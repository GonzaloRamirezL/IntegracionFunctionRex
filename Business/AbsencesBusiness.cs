using Common.DTO.GeoVictoria;
using Common.DTO.Rex;
using Common.Entity;
using Common.Enum;
using Common.Helper;
using Common.ViewModels;
using DAO.GeoVictoria;
using DAO.Rex;
using Helper;
using Helper.Extensions;
using Helpers;
using IBusiness;
using IDAO.GeoVictoria;
using IDAO.Rex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business
{
    public class AbsencesBusiness : IAbsencesBusiness
    {
        private readonly IAttendanceGeoVictoriaDAO AttendanceGeoVictoriaDAO;
        private readonly IInasistenciaRexDAO InasistenciaRexDAO;
        private readonly IProcesoAsistenciaRexDAO ProcesoAsistenciaRexDAO;
        private readonly IUserGeoVictoriaDAO UserGeoVictoriaDAO;
        private readonly IPersonaRexDAO PersonaRexDAO;
        private readonly IUserBusiness UserBusiness;
        private readonly VersionConfiguration versionConfiguration;

        private int partialUsersNumber = 40;

        public AbsencesBusiness(RexExecutionVM rexExecution)
        {
            this.AttendanceGeoVictoriaDAO = new AttendanceGeoVictoriaDAO();
            this.InasistenciaRexDAO = new InasistenciaRexDAO();
            this.ProcesoAsistenciaRexDAO = new ProcesoAsistenciaRexDAO();
            this.UserGeoVictoriaDAO = new UserGeoVictoriaDAO();
            this.PersonaRexDAO = new PersonaRexDAO();
            this.UserBusiness = new UserBusiness(rexExecution);
            this.versionConfiguration = new VersionConfiguration(rexExecution.RexVersion);
        }

        public void SynchronizeAbsences(RexExecutionVM rexExecutionVM)
        {
            List<UserVM> users = this.UserGeoVictoriaDAO.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret}).ToList();
            //users = users.FindAll(a => a.Enabled == 1);//usuarios habilitados en GV
            List<string> identifiers = users.Select(x => x.Identifier).Where(x => !string.IsNullOrEmpty(x)).ToList();

            foreach (var domain in rexExecutionVM.RexCompanyDomains)
            {
                ProcessAbsences(rexExecutionVM, identifiers, domain);
            }
        }

        public void ProcessAbsences(RexExecutionVM rexExecutionVM, List<string> users, string domain)
        {
            var firstDayDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime startProcessDate = rexExecutionVM.AbsentCalendarPeriod ? firstDayDate : rexExecutionVM.ProcessStartDate;
            DateTime endProcessDate = rexExecutionVM.AbsentCalendarPeriod ? firstDayDate.AddMonths(1).AddDays(-1) : rexExecutionVM.ProcessEndDate;
            List<Contrato> contracts = UserBusiness.GetCurrentCompanyContractsByDate(rexExecutionVM, startProcessDate, endProcessDate);
            Dictionary<string, List<Contrato>> userContractPairs = new Dictionary<string, List<Contrato>>();

            if (startProcessDate == DateTime.MinValue || endProcessDate == DateTime.MinValue) 
            {
                throw new ArgumentException("Error: No se encuentra configuracion de periodos.");
            }

            foreach (var contract in contracts)
            {
                if (contract != null &&
                    rexExecutionVM.RexCompanyCodes.Contains(contract.empresa))
                {
                    var userIdentifier = contract.empleado.Replace("-", "");
                    var contractStartDate = DateTimeHelper.StringDateTimeFileToDateTime(contract.fechaInic);
                    var contractEndDate = string.IsNullOrEmpty(contract.fechaTerm) ? null : DateTimeHelper.StringDateTimeFileToDateTime(contract.fechaTerm);
                    if ((contractEndDate == null && endProcessDate >= contractStartDate) || (contractEndDate.HasValue && contractEndDate.Value.Date >= startProcessDate && endProcessDate >= contractStartDate))
                    {
                        if (userContractPairs.TryGetValue(userIdentifier, out List<Contrato> userContracts))
                        {
                            userContracts.Add(contract);
                        }
                        else
                        {
                            userContractPairs.Add(contract.empleado.ToUpper().Replace("-", ""), new List<Contrato>() { contract });
                        }

                    }

                }
            }

            var rexAbsences = this.InasistenciaRexDAO.GetInasistencia(domain, rexExecutionVM.RexToken, startProcessDate, endProcessDate, versionConfiguration);
            for (int i = 0; i < users.Count; i += partialUsersNumber)
            {
                var partializedUserList = users.Skip(i).Take(partialUsersNumber).ToList();
                FilterCustomerContract filter = new FilterCustomerContract
                {
                    StartDate = DateTimeHelper.DateTimeToStringGeoVictoria(startProcessDate.Date),
                    EndDate = DateTimeHelper.DateTimeToStringGeoVictoria(endProcessDate.Date.AddDays(1).AddSeconds(-1)),
                    UserIds = string.Join(",", partializedUserList)
                };

                AttendanceContract attendance = this.AttendanceGeoVictoriaDAO.GetAttendanceBook(new GeoVictoriaConnectionVM()
                {
                    ApiKey = rexExecutionVM.ApiKey,
                    ApiSecret = rexExecutionVM.ApiSecret,
                    ApiToken = rexExecutionVM.ApiToken,
                    TestEnvironment = rexExecutionVM.TestEnvironment
                }, filter);

                foreach (CalculatedUser user in attendance.Users)
                {
                    user.RexContracts = userContractPairs.GetValueOrDefault(user.Identifier.ToUpper());
                    if (user.RexContracts == null)
                    {
                        user.RexContracts = new List<Contrato>();
                    }
                }

                this.Absences(rexExecutionVM, rexExecutionVM.RexCompanyDomains.First(), attendance, rexAbsences);
            }
        }

        public void Absences(RexExecutionVM rexExecutionVM, string baseURL, AttendanceContract attendance, List<PlantillaInasistencia> rexAbsences)
        {
            bool execute = false;
            bool.TryParse(ConfigurationHelper.Value("Execute"), out execute);
            Parallel.ForEach(attendance.Users, new ParallelOptions { MaxDegreeOfParallelism = 15 }, (attendanceUser) =>
            {
                var employee = attendanceUser.Identifier;
                if (rexExecutionVM.HasIDSeparator)
                {
                    employee = attendanceUser.Identifier.Substring(0, attendanceUser.Identifier.Length - 1) + "-" + attendanceUser.Identifier.Last();
                }
                var userRexAbsences = rexAbsences.FindAll(x => x.plantilla.ToUpper() == employee.ToUpper());

                var contract = attendanceUser.RexContracts.FirstOrDefault();
                if (contract == null)
                {
                    LogHelper.Log($"{attendanceUser.Identifier} Sin contrato activo!");
                }
                else
                {
                    List<LogEntity> logEntities = new List<LogEntity>();
                    foreach (var interval in attendanceUser.PlannedInterval)
                    {
                        bool isAbsent = false;
                        bool isValid = bool.TryParse(interval.Absent, out isAbsent);
                        DateTime intervalDate = DateTimeHelper.StringDateTimeToDateTime(interval.Date).Value;
                        var intervalDateTime = DateTimeHelper.DateTimeToStringRex(DateTimeHelper.StringDateTimeToDateTime(interval.Date).Value);
                        var absenceOnRex = userRexAbsences.FirstOrDefault(x => x.fechaInic == intervalDate && intervalDate == x.fechaTerm);

                        contract = attendanceUser.RexContracts.FirstOrDefault(x => DateTimeHelper.IsDateTimeInStringRange(x.fechaInic, x.fechaTerm, intervalDate, true));

                        if (DateTime.Today <= intervalDate && absenceOnRex != null)
                        {
                            if (execute)
                            {
                                this.InasistenciaRexDAO.DeleteInasistencia(baseURL, rexExecutionVM.RexToken, absenceOnRex.id, versionConfiguration);
                            }
                            logEntities.Add(LogEntityHelper.Absence(
                            rexExecutionVM,
                            LogEvent.DELETE,
                            GeneralHelper.ParseIdentifier(employee),
                            intervalDateTime + ": faltaDias",
                            LogType.Info));
                        }
                        else if (DateTime.Today > intervalDate && isValid && isAbsent && interval.Worked == "False" && contract != null && !interval.TimeOffs.Any())
                        {
                            if (absenceOnRex == null)
                            {
                                var absence = new Inasistencia()
                                {
                                    concepto = "faltaDias",
                                    contrato = contract.contrato,
                                    fechaInic = intervalDateTime,
                                    fechaTerm = intervalDateTime,
                                    valor = "1",
                                    usuario_aliado = "GeoVictoria"
                                };

                                LogHelper.Log($"{employee} - {JsonConvert.SerializeObject(absence)}");

                                if (execute)
                                {
                                    var response = this.InasistenciaRexDAO.AddInasistencia(baseURL, rexExecutionVM.RexToken, employee, absence, versionConfiguration, rexExecutionVM.HasIDSeparator);
                                    logEntities.AddRange(this.LogsFromProcessing(rexExecutionVM, employee, response, intervalDateTime));
                                }
                            }
                            else
                            {
                                //Ya se registra falta en Rex+
                            }
                        }
                        else if (absenceOnRex != null)
                        {
                            if (execute)
                            {
                                this.InasistenciaRexDAO.DeleteInasistencia(baseURL, rexExecutionVM.RexToken, absenceOnRex.id, versionConfiguration);
                            }
                            logEntities.Add(LogEntityHelper.Absence(
                            rexExecutionVM,
                            LogEvent.DELETE,
                            GeneralHelper.ParseIdentifier(employee),
                            intervalDateTime + ": faltaDias",
                            LogType.Info));
                        }

                    }
                    if (execute)
                    {
                        new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
                    }
                }                
            });

        }

        private List<LogEntity> LogsFromProcessing(RexExecutionVM rexExecution, string employee, ResponseRexMessage responseRex, string intervalDateTime)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            if (responseRex.mensajes != null && responseRex.mensajes.Count > 0)
            {
                foreach (var item in responseRex.mensajes)
                {
                    logEntities.Add(LogEntityHelper.Absence(
                        rexExecution,
                        LogEvent.ADD,
                        GeneralHelper.ParseIdentifier(employee),
                        intervalDateTime + ": " + item,
                        LogType.Error));
                }
            }
            else if (responseRex.informacion != null && responseRex.informacion.Count > 0)
            {
                foreach (var item in responseRex.informacion)
                {
                    logEntities.Add(LogEntityHelper.Absence(
                        rexExecution,
                        LogEvent.ADD,
                        GeneralHelper.ParseIdentifier(employee),
                        intervalDateTime + ": " + item,
                        LogType.Error));
                }
            }
            else if (!string.IsNullOrWhiteSpace(responseRex.detalle))
            {
                string message = responseRex.detalle.Replace("_", " ");
                    logEntities.Add(LogEntityHelper.Absence(
                        rexExecution,
                        LogEvent.ADD,
                        GeneralHelper.ParseIdentifier(employee),
                        $"{intervalDateTime} faltaDias - {message}",
                        LogType.Error));

            }
            else
            {
                logEntities.Add(LogEntityHelper.Absence(
                        rexExecution,
                        LogEvent.ADD,
                        GeneralHelper.ParseIdentifier(employee),
                        intervalDateTime + ": faltaDias",
                        LogType.Info));
            }
            return logEntities;
        }

    }
}

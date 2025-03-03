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
using System.Globalization;
using System.Linq;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace Business
{
    public class AttendanceBusiness : IAttendanceBusiness
    {
        private readonly IAttendanceGeoVictoriaDAO AttendanceGeoVictoriaDAO;
        private readonly IProcesoAsistenciaRexDAO ProcesoAsistenciaRexDAO;
        private readonly IConceptoAsistenciaRexDAO ConceptoAsistenciaRexDAO;
        private readonly IUserGeoVictoriaDAO UserGeoVictoriaDAO;
        private readonly IPersonaRexDAO PersonaRexDAO;
        private readonly IUserBusiness UserBusiness;

        private int maxUserRequest = 1500;

        private readonly VersionConfiguration versionConfiguration;

        public AttendanceBusiness(RexExecutionVM rexExecution)
        {
            this.AttendanceGeoVictoriaDAO = new AttendanceGeoVictoriaDAO();
            this.ProcesoAsistenciaRexDAO = new ProcesoAsistenciaRexDAO();
            this.ConceptoAsistenciaRexDAO = new ConceptoAsistenciaRexDAO();
            this.UserGeoVictoriaDAO = new UserGeoVictoriaDAO();
            this.PersonaRexDAO = new PersonaRexDAO();
            this.UserBusiness = new UserBusiness(rexExecution);
            this.versionConfiguration = new VersionConfiguration(rexExecution.RexVersion);

        }

        public void SynchronizeAttendanceConcepts(RexExecutionVM rexExecutionVM)
        {
            foreach (var url in rexExecutionVM.RexCompanyDomains)
            {                
                List<Contrato> contracts = UserBusiness.GetCurrentCompanyContractsByDate(rexExecutionVM, rexExecutionVM.ProcessStartDate, rexExecutionVM.ProcessEndDate);
                Dictionary<string, List<Contrato>> userContractPairs = new Dictionary<string, List<Contrato>>();
                foreach (var contract in contracts)
                {
                    if (contract != null &&
                        rexExecutionVM.RexCompanyCodes.Contains(contract.empresa))
                    {
                        var userIdentifier = contract.empleado.Replace("-", "");
                        var contractStartDate = DateTimeHelper.StringDateTimeFileToDateTime(contract.fechaInic);
                        var contractEndDate = string.IsNullOrEmpty(contract.fechaTerm) ? null : DateTimeHelper.StringDateTimeFileToDateTime(contract.fechaTerm);
                        if ((contractEndDate == null && rexExecutionVM.ProcessEndDate >= contractStartDate) || (contractEndDate.HasValue && contractEndDate.Value.Date >= rexExecutionVM.ProcessStartDate && rexExecutionVM.ProcessEndDate >= contractStartDate))
                        {
                            if (userContractPairs.TryGetValue(userIdentifier, out List<Contrato> userContracts))
                            {
                                userContracts.Add(contract);
                            }
                            else
                            {
                                userContractPairs.Add(contract.empleado.Replace("-", ""), new List<Contrato>() { contract });
                            }

                        }

                    }
                }

                var attendanceProcesses = this.GetRexAttendanceProcess(rexExecutionVM, url);
                List<Concepto> rexConcepts = new List<Concepto>();
                foreach (var idAP in attendanceProcesses)
                {
                    rexConcepts.AddRange(this.ConceptoAsistenciaRexDAO.GetConceptosAsistencia(url, rexExecutionVM.RexToken, idAP.AttendanceId, versionConfiguration));
                }
                FilterCustomerContract filter = null;
                int daysDifference = (rexExecutionVM.ProcessEndDate.Date - rexExecutionVM.ProcessStartDate.Date).Days + 1;
                int maxPartialUsersNumber = maxUserRequest / daysDifference;

                int partialUsersNumber = Math.Min(userContractPairs.Keys.Count, maxPartialUsersNumber);

                for (int i = 0; i < userContractPairs.Keys.Count; i += partialUsersNumber)
                {
                    try
                    {
                        var partializedUserList = userContractPairs.Keys.Skip(i).Take(partialUsersNumber).ToList();
                        filter = new FilterCustomerContract
                        {
                            StartDate = DateTimeHelper.DateTimeToStringGeoVictoria(rexExecutionVM.ProcessStartDate.Date),
                            EndDate = DateTimeHelper.DateTimeToStringGeoVictoria(rexExecutionVM.ProcessEndDate.Date.AddDays(1).AddSeconds(-1)),
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
                            user.RexContracts = userContractPairs.GetValueOrDefault(user.Identifier.ToUpper()) ?? new List<Contrato>();
                        }
                        foreach (ConceptVM concept in rexExecutionVM.JsonConcepts)
                        {
                            try
                            {
                                this.AttendanceConcept(rexExecutionVM, url, concept, userContractPairs.Keys.ToList(), attendance, attendanceProcesses, rexConcepts);
                            }
                            catch (Exception e)
                            {
                                var wgfa = e.Message;
                                LogHelper.Log($"ERROR {concept.Name}: {string.Join(",", attendance.Users.Select(x => x.Identifier))}");
                            }
                        }                      
                    }
                    catch (Exception e)
                    {
                        LogHelper.Log("ERROR: " + e.Message);
                    }

                }
            }
            
        }

        private void AttendanceConcept(RexExecutionVM rexExecution, string baseURL, ConceptVM concept, List<string> users, AttendanceContract attendance, List<AttendanceProcessVM> attendanceProcesses, List<Concepto> rexConceptsList)
        {
            List<Concepto> rexConcepts = new List<Concepto>();
            CultureInfo myCulture = CultureInfo.CreateSpecificCulture("en-US");
            /* 
             * Hay que cambiar esto para que tome el codigo de empresa rex correspondiente a cada usuario.
             * attendanceProcesses.FirstOrDefault().AttendanceId
             */

            switch (concept.Type)
            {
                case Common.Enum.ConceptType.Absences:
                    rexConcepts = this.Absence(concept, attendance, attendanceProcesses,myCulture);
                    break;
                case Common.Enum.ConceptType.Overtime:
                    rexConcepts = this.Overtime(concept, attendance, attendanceProcesses,myCulture);
                    break;
                case Common.Enum.ConceptType.Delay:
                    rexConcepts = this.Delay(concept, attendance, attendanceProcesses,myCulture);
                    break;
                case Common.Enum.ConceptType.EarlyLeave:
                    rexConcepts = this.EarlyLeave(concept, attendance, attendanceProcesses,myCulture);
                    break;
                case Common.Enum.ConceptType.WorkedTime:
                    rexConcepts = this.WorkedTime(concept, attendance, attendanceProcesses,myCulture);
                    break;
                case ConceptType.WorkedDays:
                    rexConcepts = this.WorkedDays(concept, attendance, attendanceProcesses,myCulture);
                    break;
                case ConceptType.NonWorkedHours:
                    rexConcepts = this.NonWorkedHours(concept, attendance, attendanceProcesses,myCulture);
                    break;
                case ConceptType.ActuallyWorkedNightHours:
                    rexConcepts = this.ActuallyWorkedNightHours(concept, attendance, attendanceProcesses, myCulture);
                    break;
                default:
                    break;
            }

            if (rexConcepts.Count > 0)
            {
                this.SyncConceptsToRex(rexExecution, baseURL, attendanceProcesses.Select(x => x.AttendanceId).ToList(), rexConcepts,rexConceptsList);
            }

        }

        public List<AttendanceProcessVM> GetRexAttendanceProcess(RexExecutionVM rexExecution, string baseURL)
        {
            bool missingProcesses = false;
            var attendanceProcesses = this.ProcesoAsistenciaRexDAO.GetProcesoAsistencia(baseURL, rexExecution.RexToken, versionConfiguration);

            foreach (var rexCompanyCode in rexExecution.RexCompanyCodes)
            {
                ProcesoAsistencia companyAttendanceProcess = attendanceProcesses.FirstOrDefault(
                    x => x.fecha_inicio == rexExecution.ProcessStartDate
                                             && x.fecha_fin == rexExecution.ProcessEndDate
                                             && x.empresa == rexCompanyCode);

                if (companyAttendanceProcess == null)
                {
                    missingProcesses = true;
                    this.ProcesoAsistenciaRexDAO.AddProcesoAsistencia(baseURL, rexExecution.RexToken, new ProcesoAsistencia()
                    {
                        fecha_inicio = rexExecution.ProcessStartDate,
                        fecha_fin = rexExecution.ProcessEndDate,
                        empresa = rexCompanyCode,
                        proceso = $"{rexExecution.ProcessEndDate.Year}-{rexExecution.ProcessEndDate.Month.ToString("d2")}"
                    }, versionConfiguration);
                }
            }

            if (missingProcesses)
            {
                attendanceProcesses = this.ProcesoAsistenciaRexDAO.GetProcesoAsistencia(baseURL, rexExecution.RexToken, versionConfiguration);
            }

            List<AttendanceProcessVM> attendances = new List<AttendanceProcessVM>();
            foreach (var process in attendanceProcesses.FindAll(x => x.fecha_inicio == rexExecution.ProcessStartDate
                                             && x.fecha_fin == rexExecution.ProcessEndDate
                                             && rexExecution.RexCompanyCodes.Contains(x.empresa)))
            {
                attendances.Add(new AttendanceProcessVM()
                {
                    AttendanceId = process.id.ToString(),
                    ProcessName = process.proceso,
                    StartDate = process.fecha_inicio,
                    EndDate = process.fecha_fin,
                    RexCompanyCode = process.empresa
                });
            }

            return attendances;
        }

        /// <summary>
        /// Valida si el intervalo se debe considerar en el calculo del valor del concepto según lo 
        /// indicando en el objeto de clase ConceptVM.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="concept"></param>
        /// <returns></returns>
        private bool ValidateInterval(TimeIntervalContract interval, ConceptVM concept, bool considerCurrentDay = false, bool includeBreaks = false)
        {
            bool result = false;
            var intervalDate = DateTimeHelper.StringGeoVictoriaToDateTime(interval.Date);

            if (intervalDate != null && 
                ((intervalDate.Value.Date < DateTime.Today) || (intervalDate.Value.Date == DateTime.Today && considerCurrentDay)) &&
                ((includeBreaks || interval.Shifts.Any(x => x.ShiftDisplay != "Break")))
               )
            {
                if (interval.Holiday == "True")
                {
                    //Si el intervalo es durante un feriado, comprobamos que el concepto abarque este dia feriado
                    if (concept.AllHolidays || (concept.Holidays != null && concept.Holidays.Contains(intervalDate.Value.DayOfWeek)))
                    {
                        result = true;
                    }
                }
                else
                {
                    //Si el intervalo no es feriado, comprobamos que el concepto abarque este dia
                    if (concept.AllCommonDays || (concept.CommonDays != null && concept.CommonDays.Contains(intervalDate.Value.DayOfWeek)))
                    {
                        result = true;
                    }
                }

                //Si el concepto indica que no se aplica a dia con Permisos, comprobamos que no existan permisos asignados
                if (concept.AllowTimeOffs == AllowTimeOffs.None && !interval.TimeOffs.IsNullOrEmpty())
                {
                    result = false;
                }

                //Si el concepto indica que solo se aplica a dia con Permisos, comprobamos que tenga al menos uno
                if (concept.AllowTimeOffs == AllowTimeOffs.Only && interval.TimeOffs.IsNullOrEmpty())
                {
                    result = false;
                }
            }

            return result;
        }

        private List<Concepto> WorkedTime(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();

            foreach (var user in attendance.Users)
            {
                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (this.ValidateInterval(interval, concept, true))
                    {
                        var workedTime = DateTimeHelper.StringToTimeSpan(interval.WorkedHours)?.TotalHours ?? 0;

                        result += workedTime;
                    }
                }

                    var contract = user.RexContracts.FirstOrDefault();
                    if (contract == null)
                    {
                        Console.WriteLine($"{user.Identifier} Sin contrato activo!");
                        continue;
                    }
                    rexConcepts.Add(
                        new Concepto()
                        {
                            contrato = contract.contrato,
                            empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                            proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                            concepto = concept.Name,
                            valor = Math.Round(result, 4).ToString(myCulture),
                            LogItem = LogItem.WORKED_TIME
                        });
            }

            return rexConcepts;
        }
        private List<Concepto> WorkedDays(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();
            foreach (var user in attendance.Users)
            {
                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (interval.Absent != "True" && this.ValidateInterval(interval, concept, true))
                    {
                        if (interval.Absent != "True" || (interval.Absent == "True" && !interval.TimeOffs.IsNullOrEmpty()))
                        {
                            result++;
                        }
                    }
                }

                    var contract = user.RexContracts.FirstOrDefault();
                    if (contract == null)
                    {
                        LogHelper.Log($"{user.Identifier} Sin contrato activo!"); 
                        continue;
                    }
                    rexConcepts.Add(
                        new Concepto()
                        {
                            contrato = contract.contrato,
                            empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                            proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                            concepto = concept.Name,
                            valor = Math.Round(result, 4).ToString(myCulture),
                            LogItem = LogItem.WORKED_TIME
                        });
            }

            return rexConcepts;
        }
        private List<Concepto> Absence(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();

            foreach (var user in attendance.Users)
            {
                var contract = user.RexContracts.FirstOrDefault();
                if (contract == null)
                {
                    Console.WriteLine($"{user.Identifier} Sin contrato activo!");
                    continue;
                }

                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (interval.Worked == "False" && this.ValidateInterval(interval, concept, false))
                    {
                        DateTime intervalDate = DateTimeHelper.StringDateTimeToDateTime(interval.Date).Value;
                        var contractForInterval = user.RexContracts.FirstOrDefault(x => DateTimeHelper.IsDateTimeInStringRange(x.fechaInic, x.fechaTerm, intervalDate, true));
                        if (contractForInterval != null)
                        {
                            result++;
                        }
                    }
                }
               
                rexConcepts.Add(
                    new Concepto()
                    {
                        contrato = contract.contrato,
                        empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                        proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                        concepto = concept.Name,
                        valor = Math.Round(result, 4).ToString(myCulture)
                    });
            }

            return rexConcepts;
        }
        private List<Concepto> Delay(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();

            foreach (var user in attendance.Users)
            {
                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (this.ValidateInterval(interval, concept, true) && interval.NonWorkedHours != "00:00")
                    {
                        var delay = DateTimeHelper.StringToTimeSpan(interval.Delay)?.TotalHours ?? 0;
                        var breakDelay = DateTimeHelper.StringToTimeSpan(interval.BreakDelay)?.TotalHours ?? 0;

                        result += delay + breakDelay;
                    }
                }

                    var contract = user.RexContracts.FirstOrDefault();
                    if (contract == null)
                    {
                        Console.WriteLine($"{user.Identifier} Sin contrato activo!");
                        continue;
                    }
                    rexConcepts.Add(
                        new Concepto()
                        {
                            contrato = contract.contrato,
                            empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                            proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                            concepto = concept.Name,
                            valor = Math.Round(result, 4).ToString(myCulture),
                            LogItem = LogItem.DELAY
                        });
            }

            return rexConcepts;
        }
        private List<Concepto> EarlyLeave(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();

            foreach (var user in attendance.Users)
            {
                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (this.ValidateInterval(interval, concept, true))
                    {
                        var earlyLeave = DateTimeHelper.StringToTimeSpan(interval.EarlyLeave)?.TotalHours ?? 0;

                        result += earlyLeave;
                    }
                }

                    var contract = user.RexContracts.FirstOrDefault();
                    if (contract == null)
                    {
                        Console.WriteLine($"{user.Identifier} Sin contrato activo!");
                        continue;
                    }
                    rexConcepts.Add(
                        new Concepto()
                        {
                            contrato = contract.contrato,
                            empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                            proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                            concepto = concept.Name,
                            valor = Math.Round(result, 4).ToString(myCulture),
                            LogItem = LogItem.EARLY_LEAVE
                        });
            }

            return rexConcepts;
        }
        private List<Concepto> Overtime(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();

            foreach (var user in attendance.Users)
            {
                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (!interval.AccomplishedExtraTime.IsNullOrEmpty() && this.ValidateInterval(interval, concept, true, true))
                    {
                        result += this.OvertimeResult(interval, concept);
                    }
                }

                var contract = user.RexContracts.FirstOrDefault();
                if (contract == null)
                {
                    Console.WriteLine($"{user.Identifier} Sin contrato activo!");
                    continue;
                }
                rexConcepts.Add(
                    new Concepto()
                    {
                        contrato = contract.contrato,
                        empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                        proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                        concepto = concept.Name,
                        valor = Math.Round(result, 4).ToString(myCulture),
                        LogItem = LogItem.OVERTIME
                    });
            }

            return rexConcepts;
        }

        private List<Concepto> ActuallyWorkedNightHours(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();
            foreach (var user in attendance.Users)
            {
                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (this.ValidateInterval(interval, concept, true))
                    {
                        var ActuallyNocturnalWorkedHours = DateTimeHelper.StringToTimeSpan(interval.ActuallyNocturnalWorkedHours)?.TotalHours ?? 0;

                        result += ActuallyNocturnalWorkedHours;
                    }
                }

                var contract = user.RexContracts.FirstOrDefault();
                if (contract == null)
                {
                    Console.WriteLine($"{user.Identifier} Sin contrato activo!");
                    continue;
                }
                rexConcepts.Add(
                    new Concepto()
                    {
                        contrato = contract.contrato,
                        empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                        proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                        concepto = concept.Name,
                        valor = Math.Round(result, 4).ToString(myCulture),
                        LogItem = LogItem.ACTUALLY_NIGHT_WORKED_TIME
                    });
            }

            return rexConcepts;
        }

        /// <summary>
        /// Obtiene la cantidad de horas extras segun lo indicado en el concepto.
        /// Las horas extras deben retornarse como decimal.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="concept"></param>
        /// <returns></returns>
        private double OvertimeResult(TimeIntervalContract interval, ConceptVM concept)
        {
            double result = 0;

            if (concept.AllowOvertime == AllowOvertime.FulfilledOvertime)
            {
                switch (concept.AllowOvertimeType)
                {
                    case AllowOvertimeType.Total:
                        result = this.FulfilledOvertime(interval.AccomplishedExtraTime, concept);
                        break;
                    case AllowOvertimeType.Before:
                        result = this.FulfilledOvertime(interval.AccomplishedExtraTimeBefore, concept);
                        break;
                    case AllowOvertimeType.After:
                        result = this.FulfilledOvertime(interval.AccomplishedExtraTimeAfter, concept);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //ToDo: Retornar tiempo extra (horas trabajadas fuera de turno)
                //Tiempo entre marca entrada y el inicio del turno.
                //Tiempo entre fin turno y marca salida.
            }

            return result;
        }

        /// <summary>
        /// Obtiene en decimal la cantidad de horas de los tiempos extras.
        /// En caso de que concepto especifique un valor de HE, se deberá obtener solo el
        /// tiempo para ese valor.
        /// </summary>
        /// <param name="overtime"></param>
        /// <param name="concept"></param>
        /// <returns></returns>
        private double FulfilledOvertime(Dictionary<string, string> overtime, ConceptVM concept)
        {
            double overtimeHours = 0;
            if (concept.OvertimeValues.IsNullOrEmpty())
            {
                overtimeHours = overtime.Values
                                        .Sum(x => DateTimeHelper.StringToTimeSpan(x)?.TotalHours ?? 0);
            }
            else
            {
                foreach (var overtimeValue in concept.OvertimeValues)
                {
                    if (overtime.TryGetValue(overtimeValue.ToString(), out string stringTime))
                    {
                        overtimeHours += DateTimeHelper.StringToTimeSpan(stringTime)?.TotalHours ?? 0;
                    }
                }

            }

            return overtimeHours;
        }
        private List<Concepto> NonWorkedHours(ConceptVM concept, AttendanceContract attendance, List<AttendanceProcessVM> rexAttendanceProcess, CultureInfo myCulture)
        {
            List<Concepto> rexConcepts = new List<Concepto>();
            foreach (var user in attendance.Users)
            {
                double result = 0;
                foreach (var interval in user.PlannedInterval)
                {
                    if (this.ValidateInterval(interval, concept, true))
                    {
                        var nonWorkedHours = DateTimeHelper.StringToTimeSpan(interval.NonWorkedHours)?.TotalHours ?? 0;

                        result += nonWorkedHours;
                    }
                }

                var contract = user.RexContracts.FirstOrDefault();
                if (contract == null)
                {
                    Console.WriteLine($"{user.Identifier} Sin contrato activo!");
                    continue;
                }
                rexConcepts.Add(
                    new Concepto()
                    {
                        contrato = contract.contrato,
                        empleado = user.Identifier.Substring(0, user.Identifier.Length - 1) + "-" + user.Identifier.Last(),
                        proceso_asistencia = rexAttendanceProcess.FirstOrDefault(x => x.RexCompanyCode == contract.empresa).AttendanceId,
                        concepto = concept.Name,
                        valor = Math.Round(result, 4).ToString(myCulture),
                        LogItem = LogItem.NON_WORKED_TIME
                    });
            }

            return rexConcepts;
        }

        private void SyncConceptsToRex(RexExecutionVM rexExecution, string baseURL, List<string> attendanceProcess, List<Concepto> concepts, List<Concepto> rexConceptsList)
        {
            List<Concepto> conceptsToAdd = new List<Concepto>();
            List<Concepto> conceptsToModify = new List<Concepto>();            

            foreach (var concept in concepts)
            {
                if (!rexExecution.HasIDSeparator)
                {
                    concept.empleado = concept.empleado.Replace("-", "");
                }
                var contractConcept = concept.contrato + ".0";
                var userConcepts = rexConceptsList.FindAll(x => x.empleado == concept.empleado);
                var rexConcept = userConcepts.FirstOrDefault(x =>
                                    x.concepto == concept.concepto &&
                                    x.proceso_asistencia == concept.proceso_asistencia 
                                    );

                decimal rexValue = 0;
                decimal gvValue = decimal.Parse(concept.valor);

                if (rexConcept != null)
                {
                    rexValue = decimal.Parse(rexConcept.valor);
                }

                if (rexConcept == null && gvValue > 0)
                {
                    conceptsToAdd.Add(concept);
                }
                else if (rexValue != gvValue)
                {
                    conceptsToModify.Add(concept);
                }
            }

            this.ExecuteSync(rexExecution, conceptsToAdd, conceptsToModify, baseURL);
        }

        private void ExecuteSync(RexExecutionVM rexExecution, List<Concepto> conceptsToAdd, List<Concepto> conceptsToModify, string baseURL)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            if (conceptsToAdd.Count > 0)
            {
                bool execute = false;
                bool.TryParse(ConfigurationHelper.Value("Execute"), out execute);
                if (!execute)
                {
                    foreach (var item in conceptsToAdd)
                    {
                        LogHelper.Log($"{item.concepto} {item.empleado} Contrato {item.contrato} Valor:{item.valor}");
                    }
                }
                else
                {
                    var addConceptsResponse = this.ConceptoAsistenciaRexDAO.AddConceptoAsistencia(baseURL, rexExecution.RexToken, conceptsToAdd, versionConfiguration);
                    if ((addConceptsResponse.mensajes != null && addConceptsResponse.mensajes.Count > 0) ||
                        (addConceptsResponse.informacion != null && addConceptsResponse.informacion.Count > 0))
                    {
                        foreach (var item in conceptsToAdd)
                        {
                            addConceptsResponse = this.ConceptoAsistenciaRexDAO.AddConceptoAsistencia(baseURL, rexExecution.RexToken, new List<Concepto>() { item }, versionConfiguration);
                            logEntities.AddRange(this.LogsFromProcessingConcepts(rexExecution, conceptsToAdd, addConceptsResponse, LogEvent.ADD));
                        }
                    }
                    else
                    {
                        logEntities.AddRange(this.LogsFromProcessingConcepts(rexExecution, conceptsToAdd, addConceptsResponse, LogEvent.ADD));
                    }
                }

            }

            if (conceptsToModify.Count > 0)
            {
                bool execute = false;
                bool.TryParse(ConfigurationHelper.Value("Execute"), out execute);
                if (!execute)
                {
                    foreach (var item in conceptsToModify)
                    {
                        LogHelper.Log($"{item.concepto} {item.empleado} Contrato {item.contrato} Valor:{item.valor}");
                    }
                }
                else
                {
                    var editConceptsResponse = this.ConceptoAsistenciaRexDAO.ModifyProcesoAsistencia(baseURL, rexExecution.RexToken, conceptsToModify, versionConfiguration);
                    if ((editConceptsResponse.mensajes != null && editConceptsResponse.mensajes.Count > 0) ||
                        (editConceptsResponse.informacion != null && editConceptsResponse.informacion.Count > 0))
                    {
                        foreach (var item in conceptsToAdd)
                        {
                            editConceptsResponse = this.ConceptoAsistenciaRexDAO.AddConceptoAsistencia(baseURL, rexExecution.RexToken, new List<Concepto>() { item }, versionConfiguration);
                            logEntities.AddRange(this.LogsFromProcessingConcepts(rexExecution, conceptsToModify, editConceptsResponse, LogEvent.EDIT));
                        }
                    }
                    else
                    {
                        logEntities.AddRange(this.LogsFromProcessingConcepts(rexExecution, conceptsToModify, editConceptsResponse, LogEvent.EDIT));
                    }
                }

            }
            if (logEntities.Count > 0)
            {
                new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
            }

        }

        private List<LogEntity> LogsFromProcessingConcepts(RexExecutionVM rexExecution, List<Concepto> concepts, ResponseRexMessage responseRex, string logEvent)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            if (responseRex.mensajes != null && responseRex.mensajes.Count > 0)
            {
                foreach (var item in responseRex.mensajes)
                {
                    logEntities.Add(LogEntityHelper.Concept(
                        rexExecution,
                        logEvent,
                        concepts.First().concepto,
                        item,
                        LogType.Error,
                        concepts.First().LogItem));
                }
            }
            else if (responseRex.informacion != null && responseRex.informacion.Count > 0)
            {
                foreach (var item in responseRex.informacion)
                {
                    logEntities.Add(LogEntityHelper.Concept(
                        rexExecution,
                        logEvent,
                        concepts.First().concepto,
                        item,
                        LogType.Error,
                        concepts.First().LogItem));
                }
            }
            else if (!string.IsNullOrWhiteSpace(responseRex.detalle))
            {
                string message = responseRex.detalle.Replace("_", " ");
                foreach (var item in concepts)
                {
                    logEntities.Add(LogEntityHelper.Concept(
                        rexExecution,
                        logEvent,
                        GeneralHelper.ParseIdentifier(item.empleado),
                        $"Concepto {item.concepto} - {message}",
                        LogType.Info,
                        item.LogItem));
                }

            }
            return logEntities;
        }

    }
}

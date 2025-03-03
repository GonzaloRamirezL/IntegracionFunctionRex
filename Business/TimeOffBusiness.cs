using Common.ViewModels;
using DAO.Rex;
using DAO.GeoVictoria;
using Helper;
using IBusiness;
using IDAO.Rex;
using IDAO.GeoVictoria;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Helpers;
using Common.Entity;
using Common.Enum;
using Common.Helper;

namespace Business
{
    public class TimeOffBusiness : ITimeOffBusiness
    {
        private const int DEFAULT_DEGREE_OF_PARALLELLISM = 10;
        private const int DAYS_INTERVAL = 1;

        private static class PermisosRex
        {
            public const string LicenciaMedica = "licenciaDias";
            public const string VacacionesNormales = "vacacionesNorm";
            public const string VacacionesProgresivas = "vacacionesProg";
            public const string PermisoSinGoce = "permisoDias";
            public const string PermisoConGoce = "permisoConDias";
            public const string LicenciaAccidentes = "accidenteDias";

            //Permiso Extras. Se crearon para CyG...?
            public const string Ley21227 = "ley21227";
            public const string Faltas = "faltaDias";
        }
        private static class PermisosGV
        {
            public const string LicenciaMedica = "#licencia#";
            public const string Vacaciones = "#vacaciones#";
            public const string PermisoSinGoce = "Permiso sin Goce";
            public const string PermisoConGoce = "Permiso con Goce";
            public const string LicenciaAccidentes = "Accidentes";

            //Permiso Extras. Se crearon para CyG...?
            public const string Ley21227 = "Dias ley 21.227";
            public const string Faltas = "Dias de Falta";
        }
        private readonly IUserGeoVictoriaDAO UserGeoVictoriaDAO;
        private readonly ITimeOffGeoVictoriaDAO TimeOffGeoVictoriaDAO;
        private readonly ITimeOffRexDAO TimeOffRexDAO;

        private readonly DateTime StartInterval;
        private readonly DateTime EndInterval;

        private readonly int MAX_PROCESS;

        private readonly VersionConfiguration versionConfiguration;

        public TimeOffBusiness(RexExecutionVM rexExecution)
        {
            this.UserGeoVictoriaDAO = new UserGeoVictoriaDAO();
            this.TimeOffGeoVictoriaDAO = new TimeOffGeoVictoriaDAO();
            this.TimeOffRexDAO = new TimeOffRexDAO();
            this.versionConfiguration = new VersionConfiguration(rexExecution.RexVersion);

            bool success = Int32.TryParse(ConfigurationHelper.Value("maxParallel"), out int maxProcess);
            MAX_PROCESS = success ? maxProcess : DEFAULT_DEGREE_OF_PARALLELLISM;
        }

        public void SynchronizeAllTimesOff(RexExecutionVM rexExecutionVM)
        {
            FilterContractRex filterR = new FilterContractRex
            {
                StartDateRequest = DateTime.UtcNow.Date.AddDays(-rexExecutionVM.TimeOffDaysRange),
                EndDateRequest = DateTime.UtcNow.Date.AddDays(rexExecutionVM.TimeOffDaysRange)
            };

            //Vacations synchronization
            List<UserVM> usersGeoVictoria = this.UserGeoVictoriaDAO.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret});
            List<TimeOffVM> vacations = SynchronizeVacations(rexExecutionVM, usersGeoVictoria, filterR);

            //Medical leaves synchronization
            List<TimeOffVM> medicalLeaves = SynchronizeMedicalLeaves(rexExecutionVM, usersGeoVictoria, filterR);

            //Administrative leaves synchronization
            List<TimeOffVM> administrativeDays = SynchronizeAdministrativeLeaves(rexExecutionVM, usersGeoVictoria, filterR);

            List<TimeOffVM> allTimesOff = new List<TimeOffVM>();
            allTimesOff.AddRange(vacations);
            allTimesOff.AddRange(medicalLeaves);
            allTimesOff.AddRange(administrativeDays);
            allTimesOff = allTimesOff.FindAll(a => a.StartDateTime >= filterR.StartDateRequest);

            SynchronizeGenericTimesOff(rexExecutionVM, allTimesOff, new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret}, filterR);

        }

        public void SynchronizeGenericTimesOff(RexExecutionVM rexExecution, List<TimeOffVM> rexTimeOffs, GeoVictoriaConnectionVM gvConnection, FilterContractRex filter)
        {
            List<TimeOffVM> gvTimeOffs = GetPermissionGeoVictoria(rexExecution, gvConnection, filter).FindAll(a => a.StartDateTime >= filter.StartDateRequest);          
            (List<TimeOffVM> newTimesOff, List<TimeOffVM> delTimesOff) = this.GetProcessedTimeOffs(rexExecution, rexTimeOffs, gvTimeOffs);
            Delete(delTimesOff, gvConnection, rexExecution);
            Add(newTimesOff, gvConnection,rexExecution);            
        }

        public List<TimeOffVM> SynchronizeVacations(RexExecutionVM rexExecutionVM, List<UserVM> usersGeoVictoria, FilterContractRex filter)
        {
            List<TimeOffVM> timesOffRex = this.TimeOffRexDAO.GetVacationsFromDateRange(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, filter, versionConfiguration);
            List<TimeOffVM> timesOff = HomologateTimesOffTypes(rexExecutionVM, timesOffRex, usersGeoVictoria, new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret});
            return timesOff;
        }
        public List<TimeOffVM> SynchronizeAdministrativeLeaves(RexExecutionVM rexExecutionVM, List<UserVM> usersGeoVictoria, FilterContractRex filter)
        {
            List<TimeOffVM> timesOffRex = this.TimeOffRexDAO.GetAdministrativeDayFromDateRange(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, filter, versionConfiguration);
            List<TimeOffVM> timesOff = HomologateTimesOffTypes(rexExecutionVM, timesOffRex, usersGeoVictoria, new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret});
            return timesOff;

        }
        public List<TimeOffVM> SynchronizeMedicalLeaves(RexExecutionVM rexExecutionVM, List<UserVM> usersGeoVictoria, FilterContractRex filter)
        {
            List<TimeOffVM> timesOffRex = this.TimeOffRexDAO.GetAbsentismFromDateRange(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, filter, versionConfiguration);
            List<TimeOffVM> timesOffHomologated = HomologateTimesOffTypes(rexExecutionVM, timesOffRex, usersGeoVictoria, new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret});
            return timesOffHomologated;
        }

        public List<TimeOffVM> GetPermissionGeoVictoria(RexExecutionVM rexExecution, GeoVictoriaConnectionVM gvConnection, FilterContractRex filter)
        {
            List<TimeOffVM> timesOffVM = this.TimeOffGeoVictoriaDAO.GetAll(gvConnection, filter);
            
            timesOffVM = this.GetNotPartialPermissionGV(rexExecution, timesOffVM, gvConnection);

            return timesOffVM;
        }

        public List<TimeOffVM> GetNotPartialPermissionGV(RexExecutionVM rexExecution, List<TimeOffVM> timeOffGV, GeoVictoriaConnectionVM gvConnection)
        {
            List<TimeOffVM> result = new List<TimeOffVM>();
            var notPartialTypes = this.GetSyncedTimeOffTypes(rexExecution, gvConnection).Where(a=> a.permisoParcial != null && a.permisoParcial != true).Select(a=>a.TimeOffTypeId).ToList();
            result = timeOffGV.FindAll(a => notPartialTypes.Contains(a.TimeOffTypeId));
            return result;
        }

        public List<TimeOffVM> GetNewPermissions(List<TimeOffVM> newTimesOff)
        {
            List<TimeOffVM> timesOff = newTimesOff.FindAll(x => (x.StartDateTime.HasValue && x.EndDateTime.HasValue)
                                                                && !(x.StartDateTime.Value.Date > this.EndInterval || x.EndDateTime.Value.Date < this.StartInterval));

            return timesOff;
        }
        public List<TimeOffVM> GetSegmentedPermissions(List<TimeOffVM> list1, List<TimeOffVM> list2)
        {
            List<TimeOffVM> newTimeOff = new List<TimeOffVM>();
            foreach (TimeOffVM tiOff in list1)
            {
                //bool exist = list2.Where(b=>b.TimeOffTypeId == tiOff.TimeOffTypeId).ToList().Any(a => a.Identifier == tiOff.Identifier && a.StartDateTime == tiOff.StartDateTime && a.EndDateTime == tiOff.EndDateTime);
                bool exist = list2.Any(a => a.Identifier == tiOff.Identifier && a.StartDateTime == tiOff.StartDateTime && a.EndDateTime == tiOff.EndDateTime);
                if (!exist)
                {
                    newTimeOff.Add(tiOff);
                }
            }
            return newTimeOff;
        }

        public (List<TimeOffVM> timeOffsToAdd, List<TimeOffVM> timeOffsToDelete) GetProcessedTimeOffs(RexExecutionVM rexExecution, List<TimeOffVM> rexTimeOffs, List<TimeOffVM> gvTimeOffs)
        {
            List<TimeOffVM> timeOffsToAdd = new List<TimeOffVM>();
            List<TimeOffVM> timeOffsToDelete = new List<TimeOffVM>();

            foreach (var rexTimeOff in rexTimeOffs)
            {
                if (!gvTimeOffs.Any(x => x.Identifier == rexTimeOff.Identifier && 
                                        x.StartDateTime == rexTimeOff.StartDateTime && 
                                        x.EndDateTime == rexTimeOff.EndDateTime))
                {
                    timeOffsToAdd.Add(rexTimeOff);
                    /*
                    var conflictingTimeOffs = gvTimeOffs.FindAll(x => x.Identifier == rexTimeOff.Identifier &&
                                            (x.StartDateTime <= rexTimeOff.StartDateTime && rexTimeOff.StartDateTime <= x.EndDateTime) ||
                                             x.StartDateTime <= rexTimeOff.EndDateTime && rexTimeOff.EndDateTime <= x.EndDateTime);

                    timeOffsToDelete.AddRange(conflictingTimeOffs);*/
                }
                
            }

            foreach (var gvTimeOff in gvTimeOffs)
            {
                if (!rexTimeOffs.Any(x => x.Identifier == gvTimeOff.Identifier &&
                                        x.StartDateTime == gvTimeOff.StartDateTime &&
                                        x.EndDateTime == gvTimeOff.EndDateTime))
                {
                    if (rexExecution.DeleteTimeOffs)
                    {
                        switch (gvTimeOff.TimeOffDescription)
                        {
                            case PermisosGV.LicenciaMedica:
                            case PermisosGV.Vacaciones:
                            case PermisosGV.PermisoSinGoce:
                            case PermisosGV.PermisoConGoce:
                            case PermisosGV.LicenciaAccidentes:
                                timeOffsToDelete.Add(gvTimeOff);
                                break;
                            default:
                                if (rexExecution.ExtraTimeOffs)
                                {
                                    timeOffsToDelete.Add(gvTimeOff);
                                }
                                else if (rexExecution.DelNotSyncTimeOff)
                                {
                                    timeOffsToDelete.Add(gvTimeOff);
                                }
                                break;
                        }
                    }
                }
            }

            timeOffsToDelete = timeOffsToDelete.GroupBy(x => x.TimeOffId)
                                               .Select(grp => grp.First())
                                               .ToList();

            return (timeOffsToAdd, timeOffsToDelete);
        }

        public void Add(List<TimeOffVM> timesOff, GeoVictoriaConnectionVM gvConnection, RexExecutionVM rexExecution)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            timesOff = timesOff.OrderBy(a => a.Identifier).ToList();
            foreach (TimeOffVM timeOff in timesOff)
            {
                (bool success, string message) = this.TimeOffGeoVictoriaDAO.Add(timeOff, gvConnection);
                logEntities.Add(LogEntityHelper.TimeOff(
                    rexExecution,
                    LogEvent.ADD,
                    timeOff.Identifier,
                    message,
                    success ? LogType.Info : LogType.Error));
            }
            new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
        }
        public void Delete(List<TimeOffVM> timesOff, GeoVictoriaConnectionVM gvConnection, RexExecutionVM rexExecution)
        {
            timesOff = timesOff.OrderBy(a => a.Identifier).ToList();
            List<LogEntity> logEntities = new List<LogEntity>();
            string separator = ConfigurationHelper.Value("Separator");
            List<string> lines = new List<string>();
            foreach (TimeOffVM timeOff in timesOff)
            {
                (bool success, string message) = this.TimeOffGeoVictoriaDAO.Delete(timeOff, gvConnection);
                logEntities.Add(LogEntityHelper.TimeOff(
                    rexExecution,
                    LogEvent.ADD,
                    timeOff.Identifier,
                    message,
                    success ? LogType.Info : LogType.Error));
                if (!success)
                {
                    string entryLine = timeOff.Identifier + separator + timeOff.TimeOffTypeId
                        + separator + timeOff.StartDateTime + separator + timeOff.EndDateTime + separator + DateTime.UtcNow;
                    lines.Add(entryLine);
                }
            }
            new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
        }

        /// <summary>
        /// Separate times off in single by day
        /// </summary>
        /// <param name="timesOff"></param>
        /// <returns></returns>
        private List<TimeOffVM> SeparateTimesOff(RexExecutionVM rexExecution, List<TimeOffVM> timesOff, GeoVictoriaConnectionVM gvConnection)
        {
            ConcurrentBag<TimeOffVM> segmentedTimesOff = new ConcurrentBag<TimeOffVM>();

            List<TimeOffTypeVM> timesOffTypes = this.GetSyncedTimeOffTypes(rexExecution, gvConnection);

            if (timesOff != null)
            {
                timesOff.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(timeOff =>
                    {
                        TimeOffTypeVM timeOffType = timesOffTypes.Find(x => x.TimeOffTypeId == timeOff.TimeOffTypeId);
                        if (timeOffType.TimeOffDescription != PermisosGV.LicenciaMedica && (timeOff.StartDateTime.HasValue && timeOff.EndDateTime.HasValue) && (timeOff.StartDateTime.Value.Date != timeOff.EndDateTime.Value.Date))
                        {
                            DateTime start = timeOff.StartDateTime.Value.Date;
                            DateTime end = timeOff.EndDateTime.Value.Date.AddDays(1).AddSeconds(-1);

                            while (start <= end.Date)
                            {
                                TimeOffVM newTimeOff = new TimeOffVM()
                                {
                                    TimeOffTypeId = timeOff.TimeOffTypeId,
                                    Identifier = timeOff.Identifier,
                                    StartDateTime = start.Date,
                                    EndDateTime = start.AddDays(1).AddSeconds(-1)
                                };

                                start = start.AddDays(1);
                                segmentedTimesOff.Add(newTimeOff);
                            }
                        }
                        else
                        {
                            TimeOffVM newTimeOff = new TimeOffVM()
                            {
                                TimeOffTypeId = timeOff.TimeOffTypeId,
                                Identifier = timeOff.Identifier,
                                StartDateTime = timeOff.StartDateTime.Value.Date,
                                EndDateTime = timeOff.EndDateTime.Value.Date.AddDays(1).AddSeconds(-1)
                            };
                            segmentedTimesOff.Add(timeOff);
                        }
                    }
                );
            }

            return segmentedTimesOff.ToList();
        }
        

        /// <summary>
        /// Homologate times off types between both systems
        /// </summary>
        /// <param name="timesOff"></param>
        /// <returns></returns>
        private List<TimeOffVM> HomologateTimesOffTypes(RexExecutionVM rexExecution, List<TimeOffVM> timesOff, List<UserVM> usersGeoVictoria, GeoVictoriaConnectionVM gvConnection)
        {
            ConcurrentBag<TimeOffVM> homologatedTimesOff = new ConcurrentBag<TimeOffVM>();
            List<TimeOffTypeVM> timesOffTypes = this.GetSyncedTimeOffTypes(rexExecution, gvConnection).FindAll(a => a.permisoParcial == false);

            timesOff.AsParallel().WithDegreeOfParallelism(MAX_PROCESS).ForAll(to =>
                {
                    TimeOffTypeVM timeOffType = null;
                    UserVM user = usersGeoVictoria.Find(x => x.Identifier.ToUpper() == to.Identifier.ToUpper() && x.Enabled == 1);
                    if (user != null)
                    {
                        if (PermisosRex.LicenciaMedica.ToUpper() == to.TimeOffDescription.ToUpper())
                        {
                            timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == PermisosGV.LicenciaMedica.ToUpper());
                        }
                        else if (PermisosRex.PermisoConGoce.ToUpper() == to.TimeOffDescription.ToUpper())
                        {
                            timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == PermisosGV.PermisoConGoce.ToUpper());
                        }
                        else if (PermisosRex.PermisoSinGoce.ToUpper() == to.TimeOffDescription.ToUpper())
                        {
                            timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == PermisosGV.PermisoSinGoce.ToUpper());
                        }
                        else if (PermisosRex.VacacionesNormales.ToUpper() == to.TimeOffDescription.ToUpper())
                        {
                            timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == PermisosGV.Vacaciones.ToUpper());
                        }
                        else if (PermisosRex.VacacionesProgresivas.ToUpper() == to.TimeOffDescription.ToUpper())
                        {
                            timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == PermisosGV.Vacaciones.ToUpper());
                        }
                        else if (PermisosRex.LicenciaAccidentes.ToUpper() == to.TimeOffDescription.ToUpper())
                        {
                            timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == PermisosGV.LicenciaAccidentes.ToUpper());
                        }
                        else if (rexExecution.ExtraTimeOffs)
                        {
                            if (PermisosRex.Ley21227.ToUpper() == to.TimeOffDescription.ToUpper())
                            {
                                timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == PermisosGV.Ley21227.ToUpper());
                            }
                            else if (PermisosRex.Faltas.ToUpper() == to.TimeOffDescription.ToUpper())
                            {
                                //Dias Falta no es un permiso. Es un dato que indica ausencia en Rex+.
                            }
                            else
                            {
                                timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == to.TimeOffDescription.ToUpper());
                            }
                        }
                        else
                        {
                            timeOffType = timesOffTypes.FirstOrDefault(x => x.TimeOffDescription.ToUpper() == to.TimeOffDescription.ToUpper());
                        }

                        if (timeOffType != null && to.StartDateTime.HasValue && to.EndDateTime.HasValue)
                        {
                            TimeOffVM timeOff = new TimeOffVM()
                            {
                                TimeOffTypeId = timeOffType.TimeOffTypeId,
                                Identifier = user.Identifier,
                                TimeOffDescription = timeOffType.TimeOffDescription,
                                StartDateTime = to.StartDateTime.Value.Date,
                                EndDateTime = to.EndDateTime.Value.AddDays(1).AddSeconds(-1),
                            };

                            homologatedTimesOff.Add(timeOff);
                        }
                    }
                }
            );

            return homologatedTimesOff.ToList();
        }

        private List<TimeOffTypeVM> GetSyncedTimeOffTypes(RexExecutionVM rexExecution, GeoVictoriaConnectionVM gvConnection)
        {
            List<TimeOffTypeVM> timesOffTypes = this.TimeOffGeoVictoriaDAO.GetAllTypes(gvConnection);
            bool newTimeOffs = false;

            if (rexExecution.ExtraTimeOffs)
            {
                if (!timesOffTypes.Any(x => x.TimeOffDescription == PermisosGV.Ley21227))
                {
                    this.TimeOffGeoVictoriaDAO.AddType(new Common.DTO.GeoVictoria.PermissionTypeContract()
                    {
                        DESCRIPCION_TIPO_PERMISO = PermisosGV.Ley21227,
                        CON_GOCE_SUELDO = true,
                        PERMISO_PARCIAL = false
                    }, gvConnection);
                    newTimeOffs = true;
                }

                if (!timesOffTypes.Any(x => x.TimeOffDescription == PermisosGV.Faltas))
                {
                    this.TimeOffGeoVictoriaDAO.AddType(new Common.DTO.GeoVictoria.PermissionTypeContract()
                    {
                        DESCRIPCION_TIPO_PERMISO = PermisosGV.Faltas,
                        CON_GOCE_SUELDO = false,
                        PERMISO_PARCIAL = false
                    }, gvConnection);
                    newTimeOffs = true;
                }
            }
            
            if (newTimeOffs)
            {
                timesOffTypes = this.TimeOffGeoVictoriaDAO.GetAllTypes(gvConnection);
            }

            return timesOffTypes;
        }
    
    }
}

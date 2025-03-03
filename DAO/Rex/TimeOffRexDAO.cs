using Common.ViewModels;
using Helper;
using IDAO.Rex;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.DTO.Rex;
using Helpers;

namespace DAO.Rex
{
    public class TimeOffRexDAO : BaseRexDAO, ITimeOffRexDAO
    {
        private const int DEFAULT_DEGREE_OF_PARALLELLISM = 10;
        public DateTime startDateRequest = DateTime.UtcNow.AddMonths(-1);
        public DateTime endDateRequest = DateTime.UtcNow.AddMonths(1);

        private readonly int MAX_PROCESS;
        public TimeOffRexDAO()
        {
            bool success = int.TryParse(ConfigurationHelper.Value("maxParallel"), out int maxProcess);
            MAX_PROCESS = success ? maxProcess : DEFAULT_DEGREE_OF_PARALLELLISM;
        }
        public List<TimeOffVM> GetVacationsFromDateRange(List<string> urls, string token, FilterContractRex filter, VersionConfiguration versionConfiguration)
        {
            string filterRex = "?fecha_inicio=" + DateTimeHelper.DateTimeToStringRex(filter.StartDateRequest) + "&fecha_fin=" + DateTimeHelper.DateTimeToStringRex(filter.EndDateRequest);

            var vacations = this.GetAll<Vacaciones>(urls, versionConfiguration.VACACIONES_URL, token, versionConfiguration, filterRex);
            List<TimeOffVM> timeOffs = ParseRexVacationsToTimeOffVM(vacations);

            return timeOffs;
        }

        public List<TimeOffVM> GetAbsentismFromDateRange(List<string> urls, string token, FilterContractRex filter, VersionConfiguration versionConfiguration)
        {
            string filterRex = "&fecha_inicio=" + DateTimeHelper.DateTimeToStringRex(filter.StartDateRequest) + "&fecha_fin=" + DateTimeHelper.DateTimeToStringRex(filter.EndDateRequest);

            var absentism = this.GetAll<Ausentismo>(urls, versionConfiguration.EMPLEADOS_PLANTILLAS_INASISTENCIAS_SPYL_URL, token, versionConfiguration, filterRex);
            List<TimeOffVM> timeOffs = ParseRexAbsentismToTimeOffVM(absentism);

            return timeOffs;
        }

        public List<TimeOffVM> GetAdministrativeDayFromDateRange(List<string> urls, string token, FilterContractRex filter,VersionConfiguration versionConfiguration)
        {
            string filterRex = "?fecha_inicio=" + DateTimeHelper.DateTimeToStringRex(filter.StartDateRequest) + "&fecha_fin=" + DateTimeHelper.DateTimeToStringRex(filter.EndDateRequest);

            var administrativeDay = this.GetAll<PermitAdministrativo>(urls, versionConfiguration.PERMISOS_ADMINISTRATIVOS_URL, token, versionConfiguration, filterRex);
            List<TimeOffVM> timeOffs = ParseRexAdministrativeDaysToTimeOffVM(administrativeDay);

            return timeOffs;
        }
       
        private List<TimeOffVM> ParseRexVacationsToTimeOffVM(List<Vacaciones> vacationsPermit)
        {
            ConcurrentBag<TimeOffVM> vacations = new ConcurrentBag<TimeOffVM>();

            if (vacationsPermit != null)
            {
                vacationsPermit.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(togv =>
                    {
                        TimeOffVM timeOf = new TimeOffVM()
                        {
                            Custom1 = togv.id,
                            TimeOffDescription = "#vacaciones#",
                            Identifier = GeneralHelper.ParseIdentifier(togv.empleado),
                            StartDateTime = DateTimeHelper.StringDateTimeFileToDateTime(togv.fechaInic),
                            EndDateTime = DateTimeHelper.StringDateTimeFileToDateTime(togv.fechaTerm)
                        };
                        vacations.Add(timeOf);
                    }
                );
            }
            return vacations.ToList();
        }
        private List<TimeOffVM> ParseRexAbsentismToTimeOffVM(List<Ausentismo> permit)
        {
            ConcurrentBag<TimeOffVM> timeOffPermit = new ConcurrentBag<TimeOffVM>();

            if (permit != null)
            {
                permit.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(togv =>
                    {
                        TimeOffVM timeOf = new TimeOffVM()
                        {                           
                            TimeOffDescription = togv.concepto,
                            Identifier = GeneralHelper.ParseIdentifier(togv.plantilla),
                            StartDateTime = DateTimeHelper.StringDateTimeFileToDateTime(togv.fechaInic),
                            EndDateTime = DateTimeHelper.StringDateTimeFileToDateTime(togv.fechaTerm)
                        };

                        timeOffPermit.Add(timeOf);
                    }
                );
            }

            return timeOffPermit.ToList();
        }
        private List<TimeOffVM> ParseRexAdministrativeDaysToTimeOffVM(List<PermitAdministrativo> administrativeDay)
        {
            ConcurrentBag<TimeOffVM> vacations = new ConcurrentBag<TimeOffVM>();

            if (administrativeDay != null)
            {
                administrativeDay.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(togv =>
                    {
                        TimeOffVM timeOf = new TimeOffVM()
                        {
                            TimeOffDescription = "P. Administrativo",
                            Identifier = GeneralHelper.ParseIdentifier(togv.empleado),
                            StartDateTime = DateTimeHelper.StringTDateTimeToDateTime(togv.fechaInicio),
                            EndDateTime = DateTimeHelper.StringTDateTimeToDateTime(togv.fechaTermino)
                        };

                        vacations.Add(timeOf);
                    }
                );
            }

            return vacations.ToList();
        }
    }
}

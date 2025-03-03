using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.GeoVictoria
{
    public class TimeIntervalContract
    {
        public string Date { get; set; }

        public List<Punch> Punches { get; set; }
        public List<Shift> Shifts { get; set; }


        public string Delay { get; set; }
        public string BreakDelay { get; set; }
        public string EarlyLeave { get; set; }


        public List<TimeOff> TimeOffs { get; set; }

        public string WorkedHours { get; set; }

        public string ActuallyNocturnalWorkedHours { get; set; } = null;

        public string Absent { get; set; }
        public string Holiday { get; set; }
        public string Worked { get; set; }

        public string NonWorkedHours { get; set; }

        public Dictionary<string, string> AccomplishedExtraTimeBefore { get; set; }

        public Dictionary<string, string> AccomplishedExtraTimeAfter { get; set; }

        public Dictionary<string, string> AccomplishedExtraTime { get; set; }

        public Dictionary<string, string> AssignedExtraTimeBefore { get; set; }
        public Dictionary<string, string> AssignedExtraTimeAfter { get; set; }
        public Dictionary<string, string> AssignedExtraTime { get; set; }

    }
    public class Shift
    {
        public string Id { get; set; }
        public string StartTime { get; set; }
        public string ExitTime { get; set; }
        public string Type { get; set; }
        public string FixedShiftHours { get; set; }
        public string Ends { get; set; }
        public string Begins { get; set; }
        public string ShiftDisplay { get; set; }
        public string BreakType { get; set; }
        public string BreakMinutes { get; set; }
        public string BreakStart { get; set; }
        public string BreakEnd { get; set; }
        public string Status { get; set; }
    }
    public class TimeOff
    {
        public string TimeOffTypeId { get; set; }
        public string Description { get; set; }
        public string Starts { get; set; }
        public string Ends { get; set; }
        public string TimeOffTypeDescription { get; set; }
        public string TimeOffOrigin { get; set; }
        public string UserIdentifier { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        /// <summary>
        /// Periodo del saldo de vacaciones ocupados en el permiso (para permisos de Feriado Legal o Progresivo)
        /// <example>2018</example>
        /// </summary>
        public string HolidayPeriod { get; set; }
        /// <summary>
        /// Código del tipo de goce de permiso (para permisos de Feriado Legal o Progresivo)
        /// <example>Legal</example>
        /// </summary>
        public string HolidayTypeCode { get; set; }
    }
    public class Punch
    {

        public string Type { get; set; }

        public string Date { get; set; }

        public string Origin { get; set; }

        public string UploadDate { get; set; }
        public string UserIdentifier { get; set; }
    }
}

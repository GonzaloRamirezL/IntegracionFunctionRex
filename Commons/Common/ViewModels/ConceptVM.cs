using Common.Enum;
using System;

namespace Common.ViewModels
{
    public class ConceptVM
    {
        public string Name { get; set; }
        public ConceptType Type { get; set; }
        public bool AllCommonDays { get; set; }
        public DayOfWeek[] CommonDays { get; set; }
        public bool AllHolidays { get; set; }
        public DayOfWeek[] Holidays { get; set; }
        public AllowTimeOffs AllowTimeOffs { get; set; }
        public AllowOvertime AllowOvertime { get; set; }
        public AllowOvertimeType AllowOvertimeType { get; set; }
        public int[] OvertimeValues { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DTO.GeoVictoria
{
    public class ShiftInsertGVContract
    {
        public string StartHour { get; set; }
        public string MaxStartHour { get; set; }
        public string EndHour { get; set; }
        public string BreakStart { get; set; }
        public string BreakEnd { get; set; }
        public int BreakMinutes { get; set; }
        public string ShiftHours { get; set; }
        public string Custom { get; set; }
        public string ShiftDay { get; set; }

    }
}

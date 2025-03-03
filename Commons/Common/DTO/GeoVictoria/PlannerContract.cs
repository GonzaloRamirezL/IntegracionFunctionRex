using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DTO.GeoVictoria
{
    public class PlannerContract
    {
        public string User { get; set; }
        public List<ShiftContract> Shift { get; set; }
    }

    public class ShiftContract
    {
        public string ShiftId { get; set; }
        public string Date { get; set; }
    }
}


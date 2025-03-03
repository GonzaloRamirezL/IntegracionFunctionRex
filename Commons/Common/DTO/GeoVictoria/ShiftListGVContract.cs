using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DTO.GeoVictoria
{
    public class ShiftListGVContract
    {
        public string ID_SHIFT { get; set; }
        public string DESCRIPTION { get; set; }
        public string TYPE_SHIFT { get; set; }
        public string START_HOUR { get; set; }
        public string END_HOUR { get; set; }
        public string START_BREAK { get; set; }
        public string END_BREAK { get; set; }
        public string BREAK_TYPE { get; set; }
        public string BREAK_MINUTES { get; set; }
        public string FOREIGN_ID { get; set; }
        public bool ENABLED { get; set; }
    }
}
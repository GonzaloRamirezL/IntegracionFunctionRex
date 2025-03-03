using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.GeoVictoria
{
    public class ConsolidatedContract
    {
        public string Identifier { get; set; }
        public string WorkedHours { get; set; }
        public string NonWorkedHours { get; set; }
        public string TotalAuthorizedExtraTime { get; set; }
        public int Absent { get; set; }
        public Dictionary<string, string> AccomplishedExtraTime { get; set; }

    }
}

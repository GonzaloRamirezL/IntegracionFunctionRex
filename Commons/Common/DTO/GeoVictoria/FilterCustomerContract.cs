using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.GeoVictoria
{
    public class FilterCustomerContract
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int IncludeAll { get; set; }
        public string UserIds { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class ProcesoAsistencia
    {
        public int id { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin { get; set; }
        public string proceso { get; set; }
        public string empresa { get; set; }
        public string origen { get; set; }
    }
    
}

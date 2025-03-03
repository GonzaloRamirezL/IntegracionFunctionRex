using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class Inasistencia
    {
        public string contrato { get; set; }
        public string concepto { get; set; }
        public string fechaInic { get; set; }
        public string fechaTerm { get; set; }
        public string valor { get; set; }
        public string usuario_aliado { get; set; }
    }
    
}

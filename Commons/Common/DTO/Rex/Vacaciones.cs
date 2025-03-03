using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class Vacaciones
    {
        public string id { get; set; }
        public string empleado { get; set; }
        public string contrato { get; set; }
        public string fechaInic { get; set; }
        public string fechaTerm { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class PermitAdministrativo
    {
        public string empleado { get; set; }
        public string contrato { get; set; }
        public string fechaInicio { get; set; }
        public string fechaTermino { get; set; }
        public string unidades { get; set; }
        public string unidad { get; set; }
        public string empresa { get; set; }
    }
}

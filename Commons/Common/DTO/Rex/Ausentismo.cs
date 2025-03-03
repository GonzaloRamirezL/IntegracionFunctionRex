using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class Ausentismo
    {
        public string concepto { get; set; }
        public string jerarquia { get; set; }
        public string origen { get; set; }
        public double valor { get; set; }
        public string fechaInic { get; set; }
        public string fechaTerm { get; set; }
        public string datoAdic { get; set; }
        public string estado { get; set; }
        public string tipo_permiso { get; set; }
        public int contrato { get; set; }
        public string plantilla { get; set; }
    }
    
}

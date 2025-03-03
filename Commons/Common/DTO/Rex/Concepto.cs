using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class PlantillaInasistencia
    {
        public int id { get; set; }
        public string concepto { get; set; }
        public double valor { get; set; }
        public DateTime fechaInic { get; set; }
        public DateTime fechaTerm { get; set; }
        public string estado { get; set; }

        /// <summary>
        /// rut empleado
        /// </summary>
        public string plantilla { get; set; }
    }
    
}

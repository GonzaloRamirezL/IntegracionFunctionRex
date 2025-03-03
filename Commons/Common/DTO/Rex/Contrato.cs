using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class Contrato
    {
        public string id { get; set; }
        public string fechaInic { get; set; }
        public string fechaTerm { get; set; }
        public string fechaCesa { get; set; }
        public string area { get; set; }
        public string empresa { get; set; }
        public string nombre { get; set; }
        public string estado { get; set; }
        public string tipoCont { get; set; }
        public string causal { get; set; }
        public string centroCost { get; set; }
        public string sede { get; set; }
        public string sindicato { get; set; }
        public string empleado { get; set; }
        public string contrato { get; set; }
        public string modalidad_contrato { get; set; }
        public string cargo { get; set; }
        public bool utiliza_asistencia { get; set; }
        public string turno { get; set; }
        public string cargo_id { get; set; }
        public string cotizacion { get; set; }

        /// <summary>
        /// Para coincidir con su respectivo empleado 
        /// en caso de empresas con multiples URL
        /// </summary>
        public string RexDomain { get; set; }
        
        /// <summary>
        /// Para revisar en logs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Contrato:{contrato};Empleado:{empleado};Ini:{fechaInic};Fin:{fechaInic};Empresa:{empresa};Modalidad:{modalidad_contrato}";
        }
    }
    
}

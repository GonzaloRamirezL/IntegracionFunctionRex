using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class Empleado
    {
        public string empleado { get; set; }
        public string nombre { get; set; }
        public string apellidoPate { get; set; }
        public string apellidoMate { get; set; }
        public string fechaNaci { get; set; }
        public string nacion { get; set; }
        public string direccion { get; set; }
        public string ciudad { get; set; }
        public string region { get; set; }
        public string numeroFono { get; set; }
        public string email { get; set; }
        public string contratoActi { get; set; }
        public string codigoInte { get; set; }
        public string empresa { get; set; }
        public string situacion { get; set; }
        public string emailPersonal { get; set; }
        public string fechaCreacion { get; set; }

        /// <summary>
        /// Para coincidir con su respectivo contrato 
        /// en caso de empresas con multiples URL
        /// </summary>
        public string RexDomain { get; set; }

        public Contrato Contrato { get; set; }
        public ObjetoCatalogo Sede { get; set; }
        public ObjetoCatalogo CentroCosto { get; set; }
        public ObjetoCatalogo Cargo { get; set; }

        public string DisableMessage { get; set; }
    }
    
}

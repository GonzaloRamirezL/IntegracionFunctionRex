using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class BadRequestRex
    {
        public string detalle { get; set; }
        public List<string> mensajes { get; set; }
        public List<string> informacion { get; set; }
    }
    
}

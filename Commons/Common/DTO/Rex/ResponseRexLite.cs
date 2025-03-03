using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class ResponseRexLite<T>
    {
        public List<T> objetos { get; set; }
    }
}

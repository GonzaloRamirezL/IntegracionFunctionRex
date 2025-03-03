using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.Rex
{
    public class ResponseRex<T>
    {
        public int cantidad_paginas { get; set; }
        public string siguiente { get; set; }
        public string anterior { get; set; }
        public List<T> objetos { get; set; }
    }
}

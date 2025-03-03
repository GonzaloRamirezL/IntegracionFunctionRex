using System;
using System.Collections.Generic;
using System.Text;

namespace Common.ViewModels
{
    public class shiftRexVM
    {
        public string idUser { get; set; } 
        public string idShiftRex { get; set; }
        public Dictionary<int, string> shifts { get; set; } 
    }
}

using Common.Enum;
using System;
using System.Collections.Generic;

namespace Common.ViewModels
{
    public class GeoVictoriaConnectionVM
    {
        public bool TestEnvironment { get; set; }
        public string ApiSecret { get; set; }
        public string ApiKey { get; set; }
        public string ApiToken { get; set; }
        
    }
}

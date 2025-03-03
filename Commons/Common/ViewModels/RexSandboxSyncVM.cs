using Common.Enum;
using System;
using System.Collections.Generic;

namespace Common.ViewModels
{
    public class RexSandboxSyncVM
    {
        public string ApiSecret { get; set; }
        public string ApiKey { get; set; }

        public string SandboxApiSecret { get; set; }
        public string SandboxApiKey { get; set; }
        public int RexVersion { get; set; } = 2;
        public bool HasSeparator { get; set; } = true;

    }
}

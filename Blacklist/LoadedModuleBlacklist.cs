using System;
using System.Collections.Generic;

namespace Wildlands_System_Scanner.Blacklist
{
    public class LoadedModuleBlacklist
    {
        public static readonly HashSet<string> Blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "malicious.dll",
            "suspiciousmodule.dll"
            // Add known malicious modules here
        };
    }
}
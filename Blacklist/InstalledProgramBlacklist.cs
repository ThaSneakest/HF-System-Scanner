using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Blacklist
{
    public class InstalledProgramBlacklist
    {
        public static readonly HashSet<string> Blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SomeMaliciousApp",
            "Fake Antivirus",
            "Unknown Software"
        };
    }
}

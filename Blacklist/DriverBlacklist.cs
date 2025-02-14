using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Blacklist
{
    public class DriverBlacklist
    {
        public static readonly List<string> Blacklist = new List<string>
        {
            "maliciousdriver.sys",
            "examplebad.sys"
            // Add other malicious driver names here
        };
    }
}

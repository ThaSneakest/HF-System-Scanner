using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Blacklist
{
    public class BlacklistedFiles
    {
        public static readonly HashSet<string> FileBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Add specific file paths to the blacklist
            "C:\\DangerousFolder\\malicious_file.exe",
            "C:\\DangerousFolder\\subfolder\\another_malicious_file.dll"
        };
    }
}

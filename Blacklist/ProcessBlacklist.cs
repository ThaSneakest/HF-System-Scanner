using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Blacklist
{
    public class ProcessBlacklist
    {
        public static readonly List<string> Blacklist = new List<string>
        {
            @"C:\users\12565\desktop\violet-v1.6\dumps\violet.exe",
            @"C:\users\12565\desktop\violet-v1.6\violet.exe",
            @"%appdata%\System\Diagnostics.exe"
        }
        .Select(ExpandEnvironmentVariables)
        .ToList(); // Normalize all paths



        public static string ExpandEnvironmentVariables(string path)
        {
            return Environment.ExpandEnvironmentVariables(path).ToLowerInvariant();
        }
    }
}

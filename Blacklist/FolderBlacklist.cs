using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Blacklist
{
    public class FolderBlacklist
    {
        public static readonly HashSet<string> BlacklistFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Add specific folder paths to the blacklist
            "C:\\DangerousFolder",
            "E:\\SuspiciousData"
        };
    }
}

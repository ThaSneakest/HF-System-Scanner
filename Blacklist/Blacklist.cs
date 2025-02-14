using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Blacklist
{
    public class Blacklist
    {
        // Example blacklist
        private static readonly HashSet<string> BlacklistedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            @"C:\Windows",
            @"C:\System32",
            @"C:\Program Files"
        };

        // Checks if the path is blacklisted
        public static bool IsBlacklisted(string path)
        {
            // Normalize the path
            string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Check if the path matches any in the blacklist
            foreach (var blacklisted in BlacklistedPaths)
            {
                if (normalizedPath.StartsWith(blacklisted, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

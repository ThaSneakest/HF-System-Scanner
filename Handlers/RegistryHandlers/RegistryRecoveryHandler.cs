using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Registry
{
    public class RegistryRecoveryHandler
    {

        // Placeholder for recovery logic handling
        public static string HandleRecovery(string key, bool isKey = true)
        {
            return isKey ? key.Replace("\\Software", "") : key;
        }

        // Converts a registry path using specific rules
        public static string RmTon(string key)
        {
            key = System.Text.RegularExpressions.Regex.Replace(key, @"(?i)(hk.+?)\\888\\", "$1\\Software\\");
            key = System.Text.RegularExpressions.Regex.Replace(key, @"(?i)(hk.+?)\\999\\", "$1\\System\\");
            return key;
        }

        // Converts a registry path using specific rules
        public static string RmTor(string key)
        {
            key = System.Text.RegularExpressions.Regex.Replace(key, @"(?i)(hk.+?)\\Software\\", "$1\\888\\");
            key = System.Text.RegularExpressions.Regex.Replace(key, @"(?i)(hk.+?)\\System\\", "$1\\999\\");
            return key;
        }
    }
}

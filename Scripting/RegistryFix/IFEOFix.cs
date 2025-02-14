using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class IFEOFix
    {
        public static void DeleteIFEO(string software)
        {
            // Define the registry key and value
            string key = $@"HKLM\{software}\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
            string value = "Debugger";

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, value);
        }

        public static void DeleteIFEOS(string fix, string software)
        {
            // Use regular expression to extract the value from FIX
            string sKey = Regex.Replace(fix, "(?i)IFEO\\\\([^:]+):.+", "$1");


            // Construct the registry key
            string key = $@"HKLM\{software}\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{sKey}";

            // Delete the registry key
            RegistryKeyHandler.DeleteRegistryKey(key);
        }
        
    }
}

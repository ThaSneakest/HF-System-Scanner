using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class InterfacesHandler
    {
        public static void DeleteInterfaces(string fix, string system, string bootSystem, string def)
        {
            // Extract the SUB value using regular expressions
            string sub = Regex.Replace(fix, @"(?i)Tcpip\\\.\\Interfaces\\([^:]+):.*", "$1");


            // Construct the registry key path
            string key = $@"HKLM\{system}{bootSystem}{def}\Services\Tcpip\Parameters\Interfaces\{sub}";

            // Extract the value to delete from the fix string
            string val = Regex.Replace(fix, @".+?\[([^\[]+)\].*", "$1");

            // Delete the value from the registry
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }
    }
}

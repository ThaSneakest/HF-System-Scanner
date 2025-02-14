using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class OpenWithProgIdsHandler
    {
        public static void DELETEOPENWITHPROGIDS(string fix)
        {
            // Extract user from the fix string using regular expression
            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\.+", "$1");

            // Assuming RELOAD and REUNLOAD are methods that handle user data
            RegistryUtils.RELOAD(user);

            // Delete the registry key
            string registryKey = $"HKU\\{user}\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.exe\\OpenWithProgids";
            RegistryKeyHandler.DeleteRegistryKey(registryKey);

            // Assuming REUNLOAD is a method to unload the registry key
            RegistryUtils.REUNLOAD(user);
        }
    }
}

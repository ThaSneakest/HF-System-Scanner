using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Browsers.Internet_Explorer
{
    public class InternetExplorerSessionFix
    {
        public static void IESESSIONFIX(string fix)
        {
            try
            {
                // Extract the user portion from the input string
                string user = Regex.Replace(fix, @"(?i).+Restore: HKU\\(.+?) ->.*", "$1");

                // Construct the registry key path
                string registryKeyPath = $@"HKEY_USERS\{user}\Software\Microsoft\Internet Explorer\ContinuousBrowsing";

                // Delete the registry key
                RegistryKeyHandler.DeleteRegistryKey(registryKeyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IESESSIONFIX: {ex.Message}");
            }
        }
    }
}

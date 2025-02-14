using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Browsers.Internet_Explorer
{
    public class InternetExplorerZoneSitesFix
    {
        public static void IEZONESITESFIX(string fix)
        {
            try
            {
                // Extract the user portion from the input string
                string user = Regex.Replace(fix, @"IE (?:trus|restric)ted site: HKU\\(.+?)\..+", "$1");

                // Extract the subdomain portion from the input string
                string subdomain = Regex.Replace(fix, @"IE.+\\(.+?) ->.*", "$1");

                // Construct the registry key path
                string registryKeyPath = $@"HKEY_USERS\{user}\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\{subdomain}";

                // Delete the registry key
                RegistryKeyHandler.DeleteRegistryKey(registryKeyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IEZONESITESFIX: {ex.Message}");
            }
        }
    }
}

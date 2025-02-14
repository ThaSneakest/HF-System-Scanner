using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class UninstallFix
    {
        public static void UNHIDEP(string fix)
        {
            // Extract the uninstall key from the fix string
            string uninstallKey = ExtractUninstallKey(fix);

            // Construct the registry key path based on the fix
            string key = ConstructRegistryKey(fix, uninstallKey);

            // Delete the 'SystemComponent' registry value
            RegistryValueHandler.DeleteRegistryValue(key, uninstallKey);
        }

        private static string ExtractUninstallKey(string fix)
        {
            // Use regex to extract the uninstall key (part after the ...\)
            var match = Regex.Match(fix, @".+\\\.\.\.\\(.+?)\).+");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        private static string ConstructRegistryKey(string fix, string uninstallKey)
        {
            string key;
            if (fix.Contains("(HKU\\"))
            {
                // Extract the user key for HKU (HKEY_USERS)
                var userMatch = Regex.Match(fix, @".+?\(HKU\\([^\\]+)\..+");
                string user = userMatch.Groups[1].Value;
                key = $@"HKU\{user}\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            }
            else
            {
                key = $@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            }

            return key;
        }
    }
}

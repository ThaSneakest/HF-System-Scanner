using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Browsers.Internet_Explorer
{
    public class InternetExplorerPrefixFix
    {
        public static void IEPREFIXFIX(string fix)
        {
            string key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\URL\";

            try
            {
                if (Regex.IsMatch(fix, "DefaultPrefix", RegexOptions.IgnoreCase))
                {
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key + "DefaultPrefix", "", "http://", RegistryValueKind.String);
                    return;
                }

                if (fix.IndexOf("[home]", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key + "Prefixes", "home", "http://", RegistryValueKind.String);
                    return;
                }

                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key + "Prefixes", "www", "http://", RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IEPREFIXFIX: {ex.Message}");
            }
        }
    }
}

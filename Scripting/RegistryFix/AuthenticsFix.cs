using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class AuthenticsFix
    {
        public static void HandleAuthenticsFix(string fix)
        {
            string subKey = Regex.Replace(fix, @".+\\(.+?): \[.+", "$1");
            string sKey = Regex.Replace(fix, @".+?: \[(.+?)\].+", "$1");

            // Delete the registry key
            string registryKey = $@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\{subKey}\{sKey}";
            RegistryKeyHandler.DeleteRegistryKey(registryKey);

            // Check if the key exists and delete it if it does
            string clsidKey = $@"HKLM\Software\Classes\CLSID\{sKey}";
            if (RegistryKeyHandler.RegistryKeyExists(clsidKey))
            {
                RegistryKeyHandler.DeleteRegistryKey(clsidKey);
            }
        }
    }
}

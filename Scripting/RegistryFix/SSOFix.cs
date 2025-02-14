using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class SSOFix
    {
        public void SSODLFIX(string fix)
        {
            string val = Regex.Replace(fix, @"SSODL: ([^\{]+) - .+ - .*", "$1");
            string key = @"HKLM\Software\Microsoft\Windows\CurrentVersion\ShellServiceObjectDelayLoad";

            // Deleting registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Check if CLSID is in the fix string
            if (Regex.IsMatch(fix, @"\{.+\}"))
            {
                string clsid = Regex.Replace(fix, @"[^\{]+(\{.+\}).*", "$1");
                key = @"HKLM\Software\Classes\CLSID\" + clsid;

                // If the key exists, delete it
                if (RegistryKeyHandler.RegistryKeyExists(key))
                {
                    RegistryKeyHandler.DeleteRegistryKey(key);
                }
            }
        }
        public void SSOFIX(string fix)
        {
            string sub = Regex.Replace(fix, @"(?i).+->\s(.+)\s=>.*", "$1");

            // Deleting registry key for ShellServiceObjects
            string key = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ShellServiceObjects\" + sub;
            RegistryKeyHandler.DeleteRegistryKey(key);

            // Deleting CLSID registry key
            key = @"HKLM\Software\Classes\CLSID\" + sub;
            if (RegistryKeyHandler.RegistryKeyExists(key))
            {
                RegistryKeyHandler.DeleteRegistryKey(key);
            }
        }
    }
}

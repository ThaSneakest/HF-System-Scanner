using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Browsers
{
    public class SearchScopeFix
    { // Function to fix search scopes in the registry
        public void SEARCHSCOPEFIX(string fix)
        {
            string key = null;
            string clsid = null;
            string user = null;

            // Default search scope registry key
            string key1 = @"\SOFTWARE\Microsoft\Internet Explorer\SearchScopes";

            // Check for the user registry (HKU)
            if (fix.Contains("HKU\\"))
            {
                // Extract user from the fix string
                user = Regex.Replace(fix, @".+HKU\\(.+?) ->.+", "$1");
                key = @"HKU\" + user + key1;
            }

            // Check for HKLM registry
            if (fix.Contains("HKLM"))
            {
                key = @"HKLM" + key1;
            }

            // If the fix involves DefaultScope, handle accordingly
            if (fix.Contains("-> DefaultScope"))
            {
                if (key.Contains(@"HKLM\"))
                {
                    // Restore DefaultScope value for HKLM
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "DefaultScope", "{0633EE93-D776-472f-A0FF-E1416B8B2E3A}", RegistryValueKind.String);
                }
                else
                {
                    // Delete DefaultScope value for other keys
                    string val = "DefaultScope";
                    RegistryValueHandler.DeleteRegistryValue(key, val);
                }
            }
            else
            {
                // Match specific SearchScopes pattern and handle
                if (Regex.IsMatch(fix, @"SearchScopes: HK(U\\.+?|LM) -> .+ URL ="))
                {
                    // Extract CLSID and delete associated registry key
                    clsid = Regex.Replace(fix, @"SearchScopes: HK(U\\.+?|LM) -> (.+) URL =.*", "$2");
                    RegistryKeyHandler.DeleteRegistryKey(key + @"\" + clsid);
                    key = @"HKLM\Software\Classes\CLSID\" + clsid;
                    if (RegistryKeyHandler.RegistryKeyExists(key))
                    {
                        RegistryKeyHandler.DeleteRegistryKey(key);
                    }

                }
                else
                {
                    // Handle case where SearchScopes doesn't have URL
                    string val = Regex.Replace(fix, @"SearchScopes: HK(U\\.+?|LM) -> ([^{]+) .*", "$2");
                    RegistryValueHandler.DeleteRegistryValue(key, val);
                }
            }
        }
    }
}

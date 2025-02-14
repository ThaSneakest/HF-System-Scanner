using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class SEHFix
    {
        public void SEHFIX(string fix)
        {
            // Extract the CLSID from the FIX string.
            string val = Regex.Replace(fix, @"[^\{]+(\{.+\}).*", "$1");

            // Registry key for ShellExecuteHooks.
            string key = "HKCR";

            // Remove the value under the registry key.
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Registry key for the CLSID.
            key = $@"HKLM\Software\Classes\CLSID\{val}";

            // If the CLSID key exists, delete it.
            if (RegistryKeyHandler.RegistryKeyExists(key))
            {
                RegistryKeyHandler.DeleteRegistryKey(key);
            }
        }
    }
}

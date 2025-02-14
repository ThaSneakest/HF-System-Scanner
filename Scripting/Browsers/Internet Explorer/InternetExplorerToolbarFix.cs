using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Browsers.Internet_Explorer
{
    public class InternetExplorerToolbarFix
    {
        public void ToolbarFix(string fix)
        {
            string key = string.Empty;
            string user = string.Empty;
            string val = string.Empty;
            string newKey = string.Empty;

            if (fix.Contains("Toolbar: HKLM "))
            {
                key = @"SOFTWARE\Microsoft\Internet Explorer\Toolbar";
            }

            if (fix.Contains("Toolbar: HKU\\"))
            {
                user = Regex.Replace(fix, @"(?i)Toolbar: HKU\\(.+?) ->.+", "$1");
                key = $@"HKU\{user}\Software\Microsoft\Internet Explorer\Toolbar\WebBrowser";
            }

            val = Regex.Replace(fix, @"[^{]+ ([!]*\{[^{]+\}).*", "$1");

            RegistryValueHandler.DeleteRegistryValue(key, val); // Delete the value for the registry key
            newKey = @"HKCU\Software\" + val;

            if (RegistryKeyHandler.RegistryKeyExists(newKey)) // Check if the key exists and delete it
            {
                RegistryKeyHandler.DeleteRegistryKey(newKey);
            }
        }
    }
}

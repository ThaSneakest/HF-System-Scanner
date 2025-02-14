using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Browsers.Internet_Explorer
{
    public class InternetExplorerURLSearchFix
    {
        public static void URLSEARCHFIX(string fix)
        {
            string ret;
            string defKey1 = @"\Software\Microsoft\Internet Explorer\URLSearchHooks";
            string clsid = "";

            string key1 = @"\Software\Microsoft\Internet Explorer\URLSearchHooks";

            if (fix.Contains("HKU\\"))
            {
                string user = Regex.Replace(fix, @".+?HKU\\(.+?) .+", "$1");
                defKey1 = @"HKU\" + user + key1;
            }

            if (fix.Contains(": HKLM "))
            {
                defKey1 = @"HKLM" + key1;
            }

            if (fix.Contains(" => "))
            {
                ret = RegistryUtilsScript.WriteRegistry(defKey1, "{CFBFAE00-17A6-11D0-99CB-00C04FD64497}", "REG_SZ", "");

                if (ret == "1")
                {
                    //Logger.WriteToLog(DEFA + " URLSearchHook " + RESTORED);
                }
                else if (ret == "0")
                {
                    // Logger.WriteToLog(NRESTORE + " " + DEFA + " URLSearchHook.");
                }
            }
            else if (fix.Contains(" -> "))
            {
                string val = "";
                RegistryValueHandler.DeleteRegistryValue(defKey1, val);
            }
            else
            {
                if (!fix.Contains("{CFBFAE00-17A6-11D0-99CB-00C04FD64497}"))
                {
                    clsid = Regex.Replace(fix, @"URLSearchHook:[^\{]+(\{.+\}).+", "$1");
                    string val = clsid;
                    RegistryValueHandler.DeleteRegistryValue(defKey1, val);

                    string key = defKey1 + "\\" + clsid;
                    if (RegistryKeyHandler.RegistryKeyExists(key)) RegistryKeyHandler.DeleteRegistryKey(key);

                    key = @"HKLM\Software\Classes\CLSID\" + clsid;
                    if (RegistryKeyHandler.RegistryKeyExists(key)) RegistryKeyHandler.DeleteRegistryKey(key);
                }
            }
        }
    }
}

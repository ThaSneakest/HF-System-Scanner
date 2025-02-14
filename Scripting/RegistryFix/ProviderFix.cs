using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class ProviderFix
    {
        public static void FixProvider(string fix)
        {
            string val = System.Text.RegularExpressions.Regex.Replace(fix, @"HKLM\\.+\\Providers\\(.+?):.*", "$1");
            string key = @"HKLM\SYSTEM\CurrentControlSet\Control\Print\Providers";
            RegistryKeyHandler.DeleteRegistryKey(key + "\\" + val);

            string order = RegistryValueHandler.ReadRegistryValue(key, "order");
            bool found = System.Text.RegularExpressions.Regex.IsMatch(order, @"(?i)(^|\n)" + val + @"(\n|$)");

            if (!found)
            {
                //    Logger.NFOUND(key + "\\order " + val);
            }
            else
            {
                string order2 = System.Text.RegularExpressions.Regex.Replace(order, @"(?i)^" + val + @"\n", "");
                order2 = System.Text.RegularExpressions.Regex.Replace(order2, @"(?i)\n" + val + @"\n", "\n");
                order2 = System.Text.RegularExpressions.Regex.Replace(order2, @"(?i)\n" + val + @"$", "");

                RegistryValueHandler.WriteRegistryValue(key, "order", order2);

                order = RegistryValueHandler.ReadRegistryValue(key, "order");
                found = System.Text.RegularExpressions.Regex.IsMatch(order, @"(?i)(^|\n)" + val + @"(\n|$)");

                if (!found)
                {
                    //     Logger.Deleted(key + "\\order " + val);
                }
                else
                {
                    //    Logger.NDELETED(key + "\\order " + val);
                }
            }
        }
    }
}

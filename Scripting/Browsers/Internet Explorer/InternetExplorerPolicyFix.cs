using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Internet_Explorer
{
    public class InternetExplorerPolicyFix
    {
        public static void IEPOLICYFIX(string fix)
        {
            try
            {
                if (Regex.IsMatch(fix, @"HKLM.+Explorer", RegexOptions.IgnoreCase))
                {
                    RegistryKeyHandler.DeleteRegistryKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Internet Explorer");
                }
                else if (Regex.IsMatch(fix, @"HKLM.+Defender", RegexOptions.IgnoreCase))
                {
                    RegistryKeyHandler.DeleteRegistryKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender");
                }
                else
                {
                    string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+", "$1");
                    RegistryKeyHandler.DeleteRegistryKey($@"HKEY_USERS\{user}\SOFTWARE\Policies\Microsoft\Internet Explorer");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IEPOLICYFIX: {ex.Message}");
            }
        }
    }
}

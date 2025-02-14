using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class NetworkServiceFix
    {
        public static void NETSVCFIX(ref string FIX, ref string SOFTWARE, ref string HFIXLOG)
        {
            try
            {
                string netSVC = (string)Microsoft.Win32.Registry.GetValue(@"HKLM\" + SOFTWARE + @"\Microsoft\Windows NT\CurrentVersion\SvcHost", "netsvcs", string.Empty);
                string val = string.Empty;

                if (FIX.Contains("->"))
                {
                    val = Regex.Replace(FIX, "(?i)NETSVC: (.+) ->.+", "$1");
                }
                else
                {
                    val = Regex.Replace(FIX, "(?i)NETSVC:[ ]*(.+)", "$1");
                }

                if (string.IsNullOrEmpty(netSVC)) return;

                bool ret1 = Regex.IsMatch(netSVC, "(?i)(^|\n)" + val + "(\n|$)");

                if (!ret1)
                {
                    //   Logger.NFOUND("HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SvcHost\\netsvcs " + val);
                }
                else
                {
                    string fix1 = Regex.Replace(netSVC, "(?i)^" + val + "\n|\n" + val + "$", "");
                    fix1 = Regex.Replace(fix1, "(?i)\n" + val + "\n", Environment.NewLine);

                    Microsoft.Win32.Registry.SetValue(@"HKLM\" + SOFTWARE + @"\Microsoft\Windows NT\CurrentVersion\SvcHost", "netsvcs", fix1, RegistryValueKind.MultiString);

                    netSVC = (string)Microsoft.Win32.Registry.GetValue(@"HKLM\" + SOFTWARE + @"\Microsoft\Windows NT\CurrentVersion\SvcHost", "netsvcs", string.Empty);

                    ret1 = Regex.IsMatch(netSVC, "(?i)(^|\n)" + val + "(\n|$)");

                    if (!ret1)
                    {
                        File.AppendAllText(HFIXLOG, "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SvcHost\\netsvcs " + val + " => " + val + " " + "Deleted" + Environment.NewLine);
                    }
                    else
                    {
                        //    Logger.NDELETED("HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SvcHost\\netsvcs " + val);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NETSVCFIX: {ex.Message}");
            }
        }
    }
}

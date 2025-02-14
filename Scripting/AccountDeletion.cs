using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;

public class AccountDeletion
{

    private static string ComputerName => Environment.MachineName;

    public static void DeleteAccount(string fix)
    {
        string ret1 = "";
        string accName = Regex.Replace(fix, @"^([^\\(]+) \\\\(.+)", "$1");


        if (Regex.IsMatch(accName, "^(?i)(DevToolsUser|HomeGroupUser\\$|sshd)$") ||
            Regex.IsMatch(fix, "(?i)S-1-5-21(-\\w+)+-50(0|1|3|4) - "))
        {
            ret1 = "Not Removed"; // Placeholder for $NREMOV
        }
        else
        {
            int ret = Netapi32NativeMethods.NetUserDel(ComputerName, accName);
            switch (ret)
            {
                case 0:
                    ret1 = "Deleted"; // Placeholder for $DELETED
                    break;
                case 2221:
                    ret1 = "Not Found"; // Placeholder for $NFOUND
                    break;
                case 5:
                    ret1 = "No Access"; // Placeholder for $NOACC
                    break;
                default:
                    ret1 = ret.ToString();
                    break;
            }
        }

        //Logger.LogFix($"{fix} + {ret1}");
    }
}

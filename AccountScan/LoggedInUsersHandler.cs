using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class LoggedInUsersHandler
    {
        public static string GetLoggedUsers()
        {
            List<string> loggedUsers = new List<string>();
            RegistryKey hkeyUsers = Microsoft.Win32.Registry.Users;

            foreach (string subKeyName in hkeyUsers.GetSubKeyNames())
            {
                if (subKeyName.Contains("S-1-5") && !Regex.IsMatch(subKeyName, @"S-1-5-(18|19|20)$|_Classes|-\{"))
                {
                    loggedUsers.Add(subKeyName);
                }
            }

            if (loggedUsers.Count == 0)
                return null;

            string loggedUsersStr = "";
            string and = " & ";

            for (int i = 0; i < loggedUsers.Count; i++)
            {
                if (i == loggedUsers.Count - 1)
                    and = "";

                string accountName = AccountHelper.SecurityLookupAccountSid(loggedUsers[i]);
                if (!string.IsNullOrEmpty(accountName))
                {
                    // Remove prefix like 'DESKTOP-<random>\'
                    accountName = Regex.Replace(accountName, @"^[^\\]+\\", "");
                    loggedUsersStr += accountName + and;
                }
            }

            return loggedUsersStr;
        }


    }
}

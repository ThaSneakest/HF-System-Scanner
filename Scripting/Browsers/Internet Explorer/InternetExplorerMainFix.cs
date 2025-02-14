using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Internet_Explorer
{
    public class InternetExplorerMainFix
    {

        public static void IEMAINFIX(string fix, int osNum, string systemDrive)
        {
            string user = string.Empty;
            string key;

            // Determine the registry key based on the input string
            if (fix.Contains(@"HKU\"))
            {
                user = Regex.Replace(fix, @"HKU\\(.+?)\\.+", "$1");
                key = $@"HKEY_USERS\{user}\Software\Microsoft\Internet Explorer\Main";
            }
            else
            {
                key = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Internet Explorer\Main";
            }

            // Extract the value name from the input string
            string val = Regex.Replace(fix, @"(?i).+Internet Explorer\\Main,([^=]+) =.*", "$1");

            // Perform operations based on the extracted value
            if (key.Contains(@"HKU\") && osNum == 10 && val == "Local Page")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "Local Page", @"%11%\blank.htm", RegistryValueKind.String);
            }
            else if (Regex.IsMatch(user, @"(?i)(\.DEFAULT|S-1-5-18|S-1-5-19|S-1-5-20)$"))
            {
                RegistryValueHandler.DeleteRegistryValue(key, val);
            }
            else if (val == "Start Page")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "Start Page", "http://go.microsoft.com/fwlink/?LinkId=69157", RegistryValueKind.String);
            }
            else if (val == "Search Page")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "Search Page", "http://go.microsoft.com/fwlink/?LinkId=54896", RegistryValueKind.String);
            }
            else if (val == "Local Page")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "Local Page", $@"{systemDrive}\Windows\System32\blank.htm", RegistryValueKind.String);
            }
            else if (val == "Default_Page_URL")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "Default_Page_URL", "http://go.microsoft.com/fwlink/?LinkId=69157", RegistryValueKind.String);
            }
            else if (val == "Default_Search_URL")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "Default_Search_URL", "http://go.microsoft.com/fwlink/?LinkId=54896", RegistryValueKind.String);
            }
            else
            {
                RegistryValueHandler.DeleteRegistryValue(key, val);
            }
        }


    }
}

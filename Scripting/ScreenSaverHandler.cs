using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner
{
    public class ScreenSaverHandler
    {
        public static void ScrnSave()
        {
            // Replace with the actual value of $FIX in AutoIt
            string fix = @"HKU\S-1-5-21-123456789-1234567890-123456789-1001\Control Panel\Desktop";
            string userSid = ExtractUserSid(fix);

            // Reload settings for the user (replace with actual logic for RELOAD in AutoIt)
            RegistryUtils.RELOAD(userSid);

            string key = $@"HKU\{userSid}\Control Panel\Desktop";
            double osVersion = Utility.GetOsVersion(); // Replace with logic to get the OS version
            string path;

            if (osVersion < 6.1)
            {
                string cDrive = "C:"; // Replace with the actual value of $C
                path = $@"{cDrive}\WINDOWS\system32\logon.scr";

                if (userSid == "S-1-5-18")
                {
                    path = $@"{cDrive}\WINDOWS\sysWOW64\logon.scr";
                }

                // Restore the registry value
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, "SCRNSAVE.EXE", path, RegistryValueKind.String);
            }
            else
            {
                // Delete the registry value
                RegistryValueHandler.DeleteRegistryValue(key, "SCRNSAVE.EXE");
            }

            // Reunload settings for the user (replace with actual logic for REUNLOAD in AutoIt)
            RegistryUtils.REUNLOAD(userSid);
        }

        private static string ExtractUserSid(string fix)
        {
            // Extract the user SID from the HKU path using a regular expression
            var match = System.Text.RegularExpressions.Regex.Match(fix, @"(?i)HKU\\(.+?)\\");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }





    }
}

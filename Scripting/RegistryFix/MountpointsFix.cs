using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class MountpointsFix
    {
        public static void MountPoints2Fix(string fix)
        {
            string clsid = string.Empty;
            string user = string.Empty;

            // Extract the user SID from the FIX string
            user = Regex.Replace(fix, @"HKU\\(.+?)\..+", "$1");

            // Check if the FIX string contains a CLSID enclosed in braces
            if (Regex.IsMatch(fix, @"\{.+\}"))
            {
                clsid = Regex.Replace(fix, @"[^\{]+(\{.+?\}).*", "$1");

                // Construct the registry key path for MountPoints2 with CLSID
                string key = @"HKU\" + user + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2\" + clsid;
                RegistryKeyHandler.DeleteRegistryKey(key);

                // Check if CLSID exists under the HKLM registry and delete it
                key = @"HKLM\Software\Classes\CLSID\" + clsid;
                if (RegistryKeyHandler.RegistryKeyExists(key))
                {
                    RegistryKeyHandler.DeleteRegistryKey(key);
                }
            }
            else
            {
                clsid = Regex.Replace(fix, @"HKU\\.+\\MountPoints2: (.+?) - .*", "$1");

                // Construct the registry key path for MountPoints2 without CLSID
                string key = @"HKU\" + user + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2\" + clsid;
                RegistryKeyHandler.DeleteRegistryKey(key);
            }
        }
    }
}

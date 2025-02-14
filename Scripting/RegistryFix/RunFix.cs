using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class RunFix
    {
        public static void DeleteRun(string fix)
        {
            // Extract the registry value name from the $FIX string
            string val = Regex.Replace(fix, @"(?i)HKLM\\.*\\Run: \[(.*)\] =>.*", "$1");

            // Construct the registry key path
            string key = $@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            // Delete the value from the registry key
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }

        public static void DeleteRunOnce(string fix)
        {
            // Extract the registry value name from the $FIX string
            string val = Regex.Replace(fix, @"(?i)HKLM\\.+\\RunOnce: \[(.*)\] =>.*", "$1");

            // Construct the registry key path
            string key = $@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce";

            // Delete the value from the registry key
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }

        public static void DeleteRunOnceEx(string fix)
        {
            // Extract the registry key name from the $FIX string
            string extractedKey = Regex.Replace(fix, @"(?i)HKLM\\...\\runonceex\\(.+?):.+", "$1");

            // Construct the full registry key path
            string key = $@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\runonceex\{extractedKey}";

            // Delete the registry key
            RegistryKeyHandler.DeleteRegistryKey(key);
        }

        public static void DeleteRunServices(string fix)
        {
            // Extract the registry value name from the $FIX string
            string val = Regex.Replace(fix, @"(?i)HKLM\\.+\\RunServices: \[(.*)\] =>.*", "$1");

            // Construct the registry key path
            string key = $@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\RunServices";

            // Delete the value from the registry key
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }

        public static void DeleteRunServicesOnce(string fix)
        {
            // Extract the registry value name from the $FIX string
            string val = Regex.Replace(fix, @"(?i)HKLM\\.+\\RunServicesOnce: \[(.*)\] =>.*", "$1");

            // Construct the registry key path
            string key = $@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\RunServicesOnce";

            // Delete the value from the registry key
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }

        public static void DeleteURun(string fix)
        {
            // Extract the registry value name from the $FIX string
            string val = Regex.Replace(fix, @"(?i)HKU\\.+\\Run: \[(.*)\] =>.*", "$1");

            // Extract the user SID from the $FIX string
            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.*", "$1");

            // Simulate RELOAD operation (replace with actual logic if needed)
            RegistryUtils.RELOAD(user);

            // Construct the registry key path
            string key = $@"HKU\{user}\Software\Microsoft\Windows\CurrentVersion\Run";

            // Delete the value from the registry key
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Simulate REUNLOAD operation (replace with actual logic if needed)
            RegistryUtils.REUNLOAD(user);
        }

        public static void DeleteURunOnce(string fix)
        {
            // Extract the registry value name from the $FIX string
            string val = Regex.Replace(fix, @"(?i)HKU\\.+\\RunOnce: \[(.*)\] =>.*", "$1");

            // Extract the user SID from the $FIX string
            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.*", "$1");

            // Simulate RELOAD operation (replace with actual logic if needed)
            RegistryUtils.RELOAD(user);

            // Construct the registry key path
            string key = $@"HKU\{user}\Software\Microsoft\Windows\CurrentVersion\RunOnce";

            // Delete the value from the registry key
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Simulate REUNLOAD operation (replace with actual logic if needed)
            RegistryUtils.REUNLOAD(user);
        }
    }
}

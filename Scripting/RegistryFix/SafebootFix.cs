using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class SafebootFix
    {
        public static void SafeBootFix(string fix)
        {
            if (fix.Contains("SafeBoot =>"))
            {
                RegistryValueHandler.RestoreRegistryValue(
                RegistryHive.LocalMachine,                               // Hive, e.g., HKEY_LOCAL_MACHINE
                    $@"SYSTEM\CurrentControlSet\Control\SafeBoot",        // Registry key path
                    "",                                                     // Value name (default value)
                    "",                                                     // Value data
                    RegistryValueKind.String                                // Value type as REG_SZ
                );

                RegistryValueHandler.RestoreRegistryValue(
                RegistryHive.LocalMachine,                               // Hive (e.g., HKEY_LOCAL_MACHINE)
                    $@"SYSTEM\CurrentControlSet\Control\SafeBoot",        // Registry key path
                    "AlternateShell",                                       // Registry value name
                    "cmd.exe",                                              // Registry value data
                    RegistryValueKind.String                                // Registry value kind (REG_SZ)
                );
            }

            if (fix.Contains("Minimal => \"\"="))
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Control\SafeBoot", "", "", RegistryValueKind.String);
            }

            if (fix.Contains("Network => \"\"="))
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Control\SafeBoot", "", "", RegistryValueKind.String);
            }

            if (fix.Contains(@"\Minimal\"))
            {
                string subKey = Regex.Replace(fix, @"(?i).+\\Minimal\\(.+) =>.*", "$1");
                string key = $@"HKLM\SYSTEM\CurrentControlSet\Control\SafeBoot\Minimal\{subKey}";
                RegistryKeyHandler.DeleteRegistryKey(key);
            }

            if (fix.Contains(@"\Network\"))
            {
                string subKey = Regex.Replace(fix, @"(?i).+\\Network\\(.+) =>.*", "$1");
                string key = $@"HKLM\SYSTEM\CurrentControlSet\Control\SafeBoot\Minimal\{subKey}";
                RegistryKeyHandler.DeleteRegistryKey(key);
            }
        }
    }
}

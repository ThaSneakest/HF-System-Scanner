using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class Drivers32Fix
    {
        public static void FixDrivers32(string fix)
        {
            const string keyPath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Drivers32";
            string name = Regex.Replace(fix, @".+?\\Drivers32:\s*\[([^]]*)\].+", "$1", RegexOptions.IgnoreCase);

            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            switch (name.ToLowerInvariant())
            {
                case string n when Regex.IsMatch(n, @"^(aux\d?|midi\d?|mixer\d?|wave\d?)$", RegexOptions.IgnoreCase):
                    RegistryValueHandler.RestoreRegistryValue(
                        RegistryHive.LocalMachine, // Specify the hive, e.g., HKLM
                        keyPath,                   // Path to the registry key
                        name,                      // Name of the value to restore
                        "wdmaud.drv",              // Value data
                        RegistryValueKind.String   // Value kind
                    );

                    break;

                case "midimapper":
                    RegistryValueHandler.RestoreRegistryValue(
                        RegistryHive.LocalMachine, // Specify the registry hive
                        keyPath,                   // Path to the registry key
                        name,                      // Name of the registry value
                        "midimap.dll",             // Data to be restored
                        RegistryValueKind.String   // Registry value type ("REG_SZ")
                    );

                    break;

                case "msvideo8":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "VfWWDM32.dll", RegistryValueKind.String);
                    break;

                case "msacm.imaadpcm":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "imaadp32.acm", RegistryValueKind.String);
                    break;

                case "msacm.l3acm":
                case "msacm.l3codecp":
                    RegistryValueHandler.RestoreRegistryValue(
                        RegistryHive.LocalMachine,     // Registry hive, e.g., HKLM
                        keyPath,                       // Path to the registry key
                        name,                          // Name of the registry value
                        $"{defaultPath}\\l3codeca.acm", // Value to be set
                        RegistryValueKind.String       // Specifies the type as REG_SZ
                    );

                    break;

                case "msacm.l3acmp":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, $"{defaultPath}\\l3codecp.acm", RegistryValueKind.String);
                    break;

                case "msacm.msadpcm":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "msadp32.acm", RegistryValueKind.String);
                    break;

                case "msacm.msg711":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "msg711.acm", RegistryValueKind.String);
                    break;

                case "msacm.msgsm610":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "msgsm32.acm", RegistryValueKind.String);
                    break;

                case string n when Regex.IsMatch(n, @"^(vidc.i420|vidc.iyuv)$", RegexOptions.IgnoreCase):
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "iyuv_32.dll", RegistryValueKind.String);
                    break;

                case "vidc.mrle":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "msrle32.dll", RegistryValueKind.String);
                    break;

                case "vidc.msvc":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "msvidc32.dll", RegistryValueKind.String);
                    break;

                case "vidc.pdad":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "prodad-codec.dll", RegistryValueKind.String);
                    break;

                case string n when Regex.IsMatch(n, @"^(vidc.uyvy|vidc.yuy2|vidc.yvyu)$", RegexOptions.IgnoreCase):
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "msyuv.dll", RegistryValueKind.String);
                    break;

                case "vidc.yvu9":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "tsbyuv.dll", RegistryValueKind.String);
                    break;

                case "wavemapper":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "msacm32.drv", RegistryValueKind.String);
                    break;

                case "msacm.siren":
                    RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, name, "sirenacm.dll", RegistryValueKind.String);
                    break;

                default:
                    RegistryValueHandler.DeleteRegistryValue(keyPath, name);
                    break;
            }
        }

        public static void Drivers32UFIX(string fix)
        {
            // Extract the user SID from the fix string
            string userSid = Regex.Replace(fix, @"HKU\\(.+?)\\.+", "$1", RegexOptions.IgnoreCase);

            // Construct the full registry key path
            string keyPath = $@"HKU\{userSid}\Software\Microsoft\Windows NT\CurrentVersion\Drivers32";

            // Extract the name of the value to delete
            string valueName = Regex.Replace(fix, @".+?\\Drivers32:\s*\[([^]]*)\].+", "$1", RegexOptions.IgnoreCase);

            try
            {
                // Open the registry key
                using (RegistryKey key = Microsoft.Win32.Registry.Users.OpenSubKey(keyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {keyPath}");
                        return;
                    }

                    // Check if the value exists and delete it
                    if (key.GetValue(valueName) != null)
                    {
                        key.DeleteValue(valueName);
                        Console.WriteLine($"Deleted value: {valueName} from {keyPath}");
                    }
                    else
                    {
                        Console.WriteLine($"Value not found: {valueName} in {keyPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

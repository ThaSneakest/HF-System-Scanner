using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Scripting;
using Wildlands_System_Scanner.Utilities;

//Tested and working 

namespace Wildlands_System_Scanner
{
    public class CommandProcessorHandler
    {
        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Command Processor", "SafeValueName", "SafeData"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Command Processor\Autorun", "SafeStartup", "SafeCommand"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Command Processor\Autorun", "UserSafeValue", "UserSafeCommand"),
            // Add more whitelist entries as needed
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Command Processor", "Autorun", @"C:\Wildlands\Test.dll"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\Command Processor\Autorun", "", "cmd /c evil.bat"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Command Processor\Autorun", "", "bad_script.cmd"),
            // Add more blacklist entries as needed
        };

        public static void ScanCommandProcessor()
        {
            string[] registryPaths = new[]
            {
                @"Software\Microsoft\Command Processor",
                @"Software\Microsoft\Command Processor\Autorun",
                @"Software\Wow6432Node\Microsoft\Command Processor\Autorun"
            };

            Console.WriteLine($"Starting scan for Command Processor registry paths...");

            // Scan HKLM and HKCU
            foreach (string path in registryPaths)
            {
                ScanKey(Microsoft.Win32.Registry.LocalMachine, path);
                ScanKey(Microsoft.Win32.Registry.CurrentUser, path);
            }

            // Scan HKU for all user subkeys
            ScanAllUserKeys();
        }

        private static void ScanKey(RegistryKey baseRegistry, string subPath)
        {
            string fullKeyPath = $@"{baseRegistry.Name}\{subPath}";
            Console.WriteLine($"Scanning: {fullKeyPath}");

            try
            {
                using (RegistryKey baseKey = baseRegistry.OpenSubKey(subPath, false)) // Open in read-only mode
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {fullKeyPath}");
                        return;
                    }

                    Console.WriteLine($"Opened registry key: {fullKeyPath}");

                    // Enumerate values of the key
                    foreach (string valueName in baseKey.GetValueNames())
                    {
                        string valueData = baseKey.GetValue(valueName)?.ToString() ?? "NULL";

                        // Check whitelist
                        if (Whitelist.Contains(Tuple.Create(fullKeyPath, valueName, valueData)))
                        {
                            Console.WriteLine($"Whitelisted entry skipped: {fullKeyPath}: [{valueName}] -> {valueData}");
                            continue;
                        }

                        // Check blacklist
                        string attn = Blacklist.Contains(Tuple.Create(fullKeyPath, valueName, valueData))
                            ? " <==== Malicious Registry Entry Found"
                            : string.Empty;

                        Logger.Instance.LogPrimary($"{fullKeyPath}: [{valueName}] -> {valueData}{attn}");
                    }

                    // Enumerate and scan subkeys recursively
                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        string subKeyFullPath = $@"{subPath}\{subKeyName}";
                        ScanKey(baseRegistry, subKeyFullPath);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"ACCESS DENIED: {fullKeyPath} - Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning {fullKeyPath}: {ex.Message}");
            }
        }

        private static void ScanAllUserKeys()
        {
            Console.WriteLine("Scanning HKEY_USERS...");

            try
            {
                using (RegistryKey usersRoot = Microsoft.Win32.Registry.Users)
                {
                    foreach (string userSid in usersRoot.GetSubKeyNames())
                    {
                        string userKeyPath = $@"{userSid}\Software\Microsoft\Command Processor";
                        ScanKey(Microsoft.Win32.Registry.Users, userKeyPath);
                        ScanKey(Microsoft.Win32.Registry.Users, $@"{userKeyPath}\Autorun");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("ACCESS DENIED: HKEY_USERS - Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning HKEY_USERS: {ex.Message}");
            }
        }
    }
}

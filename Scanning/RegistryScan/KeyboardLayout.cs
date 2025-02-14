using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class KeyboardLayout
    {
        private static readonly string[] RegistryPaths =
 {
            @"Keyboard Layout\Preload"
        };

        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_CURRENT_USER\Keyboard Layout\Preload", "1", "00000409"), // English (US)
            Tuple.Create(@"HKEY_USERS\.DEFAULT\Keyboard Layout\Preload", "1", "00000409"),
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_CURRENT_USER\Keyboard Layout\Preload", "1", "00000000"), // Invalid layout
            Tuple.Create(@"HKEY_USERS\.DEFAULT\Keyboard Layout\Preload", "1", "99999999"), // Fake layout
        };

        public static void ScanKeyboardLayout()
        {
            Console.WriteLine("Starting scan for Keyboard Layout Preload registry keys...");

            // Scan HKCU
            ScanKey(Microsoft.Win32.Registry.CurrentUser, RegistryPaths[0], "HKEY_CURRENT_USER");

            // Scan HKU (Enumerate all user profiles)
            using (RegistryKey usersKey = Microsoft.Win32.Registry.Users)
            {
                foreach (string sid in usersKey.GetSubKeyNames())
                {
                    string userPath = $@"{sid}\{RegistryPaths[0]}";
                    ScanKey(usersKey, userPath, $"HKEY_USERS\\{sid}");
                }
            }

            Console.WriteLine("Scan complete.");
        }

        private static void ScanKey(RegistryKey baseRegistry, string subPath, string rootName)
        {
            string fullKeyPath = $@"{rootName}\{subPath}";
            Console.WriteLine($"Scanning: {fullKeyPath}");

            try
            {
                using (RegistryKey baseKey = baseRegistry.OpenSubKey(subPath, false)) // Read-only mode
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {fullKeyPath}");
                        return;
                    }

                    Console.WriteLine($"Opened registry key: {fullKeyPath}");

                    // Enumerate values
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

                    // Enumerate subkeys recursively
                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        string subKeyFullPath = $@"{subPath}\{subKeyName}";
                        ScanKey(baseRegistry, subKeyFullPath, rootName);
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
    }
}

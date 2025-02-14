using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class AppContainer
    {
        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_USERS\{userKey}\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer", "ExampleValue", "SafeData"),
            // Add additional known safe values here
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_USERS\{userKey}\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer", "MaliciousValue", "MaliciousData"),
            // Add additional malicious/suspicious values here
        };

        public static void ScanAppContainer()
        {
            try
            {
                using (RegistryKey usersKey = Microsoft.Win32.Registry.Users)
                {
                    foreach (string userKey in usersKey.GetSubKeyNames())
                    {
                        string basePath = $@"{userKey}\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer";
                        ScanKey(usersKey, basePath, userKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning HKU: {ex.Message}");
            }
        }

        private static void ScanKey(RegistryKey baseRegistry, string subPath, string userKey)
        {
            string fullKeyPath = $@"{baseRegistry.Name}\{subPath}".Replace("{userKey}", userKey);
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
                        ScanKey(baseRegistry, subKeyFullPath, userKey);
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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class Filter
    {
        // Define registry paths to scan
        private static readonly string[] RegistryPaths =
        {
            @"Software\Classes\Filter",
            @"Software\Wow6432Node\Classes\Filter",
            @"Software\Classes\Filter"
        };

        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Classes\Filter", "Default", "SafeFilter"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Classes\Filter", "Default", "SafeFilter"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Classes\Filter", "Default", "SafeFilter"),
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Classes\Filter", "MaliciousFilter", "EvilFilter"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Classes\Filter", "MaliciousFilter", "EvilFilter"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Classes\Filter", "MaliciousFilter", "EvilFilter"),
        };

        public static void ScanFilterRegistry()
        {
            Console.WriteLine("Starting scan for Filter registry paths...");

            // Scan HKLM and HKCU for defined paths
            foreach (string path in RegistryPaths)
            {
                ScanKey(Microsoft.Win32.Registry.LocalMachine, path, "HKEY_LOCAL_MACHINE");
                ScanKey(Microsoft.Win32.Registry.CurrentUser, path, "HKEY_CURRENT_USER");
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

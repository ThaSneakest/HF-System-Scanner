using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class AppCompatFlags
    {
        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags", "ExampleValue", "SafeData"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags", "ExampleValue", "SafeData"),
            // Add additional known safe values here
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags", "Test", @"C:\wildlands\test.dll"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags", "SuspiciousEntry", "HackerTool.exe"),
            // Add additional malicious/suspicious values here
        };

        public static void ScanAppCompatFlags()
        {
            string[] registryPaths = new[]
            {
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\TelemetryController",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Custom",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\InstalledSDB",
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags",
                @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags"
            };

            // Scan HKLM
            foreach (string path in registryPaths)
            {
                ScanKey(Microsoft.Win32.Registry.LocalMachine, path);
            }

            // Scan HKCU
            foreach (string path in registryPaths)
            {
                ScanKey(Microsoft.Win32.Registry.CurrentUser, path);
            }
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
    }
}

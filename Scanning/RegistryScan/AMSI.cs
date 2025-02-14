using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class AMSI
    {
        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\AMSI", "FeatureBits", "1"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\AMSI", "DebugMode", "0"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\AMSI\Provider", "Path", @"C:\Windows\System32\amsi.dll"),
            // Add more whitelist entries if necessary
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\AMSI", "Test", @"C:\Wildlands\Test.dll"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\AMSI\Provider", "Path", @"C:\Malicious\malicious_amsi.dll"),
            // Add more blacklist entries if necessary
        };

        public static void ScanAMSI()
        {
            string registryPath = @"SOFTWARE\Microsoft\AMSI";
            Console.WriteLine($"Starting scan for registry path: HKLM\\{registryPath}");

            try
            {
                ScanKey(Microsoft.Win32.Registry.LocalMachine, registryPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while scanning AMSI: {ex.Message}");
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


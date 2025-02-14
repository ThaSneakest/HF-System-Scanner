using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning
{
    public class RegistryBlacklistScan
    {
        public class RegistryEntry
        {
            public string Key { get; }
            public string SubKey { get; }
            public string Value { get; }
            public string ValueData { get; }

            public RegistryEntry(string key, string subKey, string value, string valueData)
            {
                Key = key?.Trim() ?? string.Empty;
                SubKey = subKey?.Trim() ?? string.Empty;
                Value = value?.Trim() ?? string.Empty;
                ValueData = valueData?.Trim() ?? string.Empty;
            }

            public override bool Equals(object obj)
            {
                if (obj is RegistryEntry other)
                {
                    return string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(SubKey, other.SubKey, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(ValueData, other.ValueData, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (Key?.ToLowerInvariant().GetHashCode() ?? 0);
                    hash = hash * 23 + (SubKey?.ToLowerInvariant().GetHashCode() ?? 0);
                    hash = hash * 23 + (Value?.ToLowerInvariant().GetHashCode() ?? 0);
                    hash = hash * 23 + (ValueData?.ToLowerInvariant().GetHashCode() ?? 0);
                    return hash;
                }
            }
        }

        // Blacklists
        private static readonly HashSet<string> BlacklistedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
           "HKEY_LOCAL_MACHINE", "HKEY_CURRENT_USER"
        };

        private static readonly HashSet<string> BlacklistedSubKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Software\\MaliciousApp", "System\\ControlSet001\\Services\\SuspiciousService"
            };

        private static readonly HashSet<string> BlacklistedValues =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "EnableFeature", "RunOnStartup" };

        private static readonly HashSet<string> BlacklistedValueData =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "1", "yes", "true" };

        private static readonly HashSet<RegistryEntry> BlacklistedEntries = new HashSet<RegistryEntry>
        {
            new RegistryEntry("HKEY_USERS", "Default\\Software\\HackTool", "Installed", "true")
        };

        // Checks if an entry is blacklisted
        public static bool IsBlacklisted(string key, string subKey, string value, string valueData)
        {
            if (BlacklistedKeys.Contains(key) ||
                BlacklistedSubKeys.Contains(subKey) ||
                BlacklistedValues.Contains(value) ||
                BlacklistedValueData.Contains(valueData) ||
                BlacklistedEntries.Contains(new RegistryEntry(key, subKey, value, valueData)))
            {
                return true;
            }

            return false;
        }

        // Scans the registry for blacklisted entries and logs malicious ones
        public static List<string> ScanRegistry()
        {
            var log = new List<string>();

            foreach (var rootKey in new[] { Microsoft.Win32.Registry.LocalMachine, Microsoft.Win32.Registry.CurrentUser, Microsoft.Win32.Registry.Users })
            {
                ScanRegistryKey(rootKey, string.Empty, log);
            }

            return log;
        }

        private static void ScanRegistryKey(RegistryKey parentKey, string parentPath, List<string> log)
        {
            try
            {
                foreach (var subKeyName in parentKey.GetSubKeyNames())
                {
                    var subKeyPath = string.IsNullOrEmpty(parentPath) ? subKeyName : $"{parentPath}\\{subKeyName}";
                    using (var subKey = parentKey.OpenSubKey(subKeyName))
                    {
                        if (subKey == null) continue;

                        // Check the subkey against the blacklist
                        if (IsBlacklisted(parentKey.Name, subKeyPath, string.Empty, string.Empty))
                        {
                            log.Add($"{parentKey.Name}\\{subKeyPath} <---- Malicious Entry Found");
                        }

                        // Check values within the subkey
                        foreach (var valueName in subKey.GetValueNames())
                        {
                            var valueData = subKey.GetValue(valueName)?.ToString() ?? string.Empty;

                            if (IsBlacklisted(parentKey.Name, subKeyPath, valueName, valueData))
                            {
                                log.Add(
                                    $"{parentKey.Name}\\{subKeyPath} -> {valueName} = {valueData} <---- Malicious Entry Found");
                            }
                        }

                        // Recursively scan subkeys
                        ScanRegistryKey(subKey, subKeyPath, log);
                    }
                }
            }
            catch
            {
                // Handle permission errors silently
            }
        }

        // Outputs the current blacklist
        public static void PrintBlacklist()
        {
            Console.WriteLine("Blacklisted Keys:");
            foreach (var key in BlacklistedKeys)
            {
                Console.WriteLine($"Key: {key}");
            }

            Console.WriteLine("\nBlacklisted SubKeys:");
            foreach (var subKey in BlacklistedSubKeys)
            {
                Console.WriteLine($"SubKey: {subKey}");
            }

            Console.WriteLine("\nBlacklisted Values:");
            foreach (var value in BlacklistedValues)
            {
                Console.WriteLine($"Value: {value}");
            }

            Console.WriteLine("\nBlacklisted Value Data:");
            foreach (var valueData in BlacklistedValueData)
            {
                Console.WriteLine($"ValueData: {valueData}");
            }

            Console.WriteLine("\nBlacklisted Full Entries:");
            foreach (var entry in BlacklistedEntries)
            {
                Console.WriteLine(
                    $"Key: {entry.Key}, SubKey: {entry.SubKey}, Value: {entry.Value}, ValueData: {entry.ValueData}");
            }
        }
    }
}
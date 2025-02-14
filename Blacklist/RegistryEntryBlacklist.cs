using System;
using System.Collections.Generic;

namespace Wildlands_System_Scanner.Blacklist
{
    public class RegistryEntryBlacklist
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

        // Separate blacklists for keys, subkeys, values, value data, and full entries
        private static readonly HashSet<string> BlacklistedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HKEY_LOCAL_MACHINE",
            "HKEY_CURRENT_USER"
        };

        private static readonly HashSet<string> BlacklistedSubKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Software\\MaliciousApp",
            "System\\ControlSet001\\Services\\SuspiciousService"
        };

        private static readonly HashSet<string> BlacklistedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "EnableFeature",
            "RunOnStartup"
        };

        private static readonly HashSet<string> BlacklistedValueData = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "1",
            "yes",
            "true"
        };

        private static readonly HashSet<RegistryEntry> BlacklistedEntries = new HashSet<RegistryEntry>
        {
            new RegistryEntry("HKEY_USERS", "Default\\Software\\HackTool", "Installed", "true")
        };

        // Checks if any part of a registry entry is blacklisted
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

        // Processes a log to mark blacklisted entries
        public static List<string> ProcessLog(List<string> log)
        {
            var updatedLog = new List<string>();

            foreach (var entry in log)
            {
                // Parse the log entry
                var parsedEntry = ParseLogEntry(entry);
                if (parsedEntry == null)
                {
                    updatedLog.Add(entry); // Add unparseable entries as-is
                    continue;
                }

                string key = parsedEntry[0];
                string subKey = parsedEntry[1];
                string value = parsedEntry[2];
                string valueData = parsedEntry[3];

                // Check if the entry is blacklisted
                if (IsBlacklisted(key, subKey, value, valueData))
                {
                    updatedLog.Add($"{entry} <---- Possible Malicious Entry");
                }
                else
                {
                    updatedLog.Add(entry);
                }
            }

            return updatedLog;
        }

        // Parses a log entry into registry entry components
        private static string[] ParseLogEntry(string logEntry)
        {
            // Assume the log format is "Key\\SubKey -> Value = ValueData"
            // Example: "HKEY_LOCAL_MACHINE\\Software\\MaliciousApp -> EnableFeature = 1"
            try
            {
                var parts = logEntry.Split(new[] { " -> ", " = " }, StringSplitOptions.None);
                if (parts.Length == 3)
                {
                    var keyParts = parts[0].Split(new[] { '\\' }, 2);
                    return new[] { keyParts[0], keyParts[1], parts[1], parts[2] };
                }
            }
            catch
            {
                // Return null if the log entry cannot be parsed
            }

            return null;
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
                Console.WriteLine($"Key: {entry.Key}, SubKey: {entry.SubKey}, Value: {entry.Value}, ValueData: {entry.ValueData}");
            }
        }
    }
}

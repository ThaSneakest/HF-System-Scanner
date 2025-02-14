using System;
using System.Collections.Generic;

namespace Wildlands_System_Scanner.Whitelist
{
    public class RegistryEntryWhitelist
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

        // Separate whitelists for keys, subkeys, values, value data, and full entries
        private static readonly HashSet<string> WhitelistedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HKEY_LOCAL_MACHINE",
            "HKEY_CURRENT_USER"
        };

        private static readonly HashSet<string> WhitelistedSubKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Software\\TrustedApp",
            "System\\ControlSet001\\Services\\SafeService"
        };

        private static readonly HashSet<string> WhitelistedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AllowFeature",
            "RunSafely"
        };

        private static readonly HashSet<string> WhitelistedValueData = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "0",
            "no",
            "false"
        };

        private static readonly HashSet<RegistryEntry> WhitelistedEntries = new HashSet<RegistryEntry>
        {
            new RegistryEntry("HKEY_USERS", "Default\\Software\\TrustedTool", "Installed", "false")
        };

        // Checks if a registry entry is whitelisted
        public static bool IsWhitelisted(string key, string subKey, string value, string valueData)
        {
            if (WhitelistedKeys.Contains(key) ||
                WhitelistedSubKeys.Contains(subKey) ||
                WhitelistedValues.Contains(value) ||
                WhitelistedValueData.Contains(valueData) ||
                WhitelistedEntries.Contains(new RegistryEntry(key, subKey, value, valueData)))
            {
                return true;
            }
            return false;
        }

        // Processes a log to mark whitelisted entries
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

                // Check if the entry is whitelisted
                if (IsWhitelisted(key, subKey, value, valueData))
                {
                    updatedLog.Add($"{entry} <---- Whitelisted Entry");
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
            // Example: "HKEY_LOCAL_MACHINE\\Software\\TrustedApp -> AllowFeature = 0"
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

        // Outputs the current whitelist
        public static void PrintWhitelist()
        {
            Console.WriteLine("Whitelisted Keys:");
            foreach (var key in WhitelistedKeys)
            {
                Console.WriteLine($"Key: {key}");
            }

            Console.WriteLine("\nWhitelisted SubKeys:");
            foreach (var subKey in WhitelistedSubKeys)
            {
                Console.WriteLine($"SubKey: {subKey}");
            }

            Console.WriteLine("\nWhitelisted Values:");
            foreach (var value in WhitelistedValues)
            {
                Console.WriteLine($"Value: {value}");
            }

            Console.WriteLine("\nWhitelisted Value Data:");
            foreach (var valueData in WhitelistedValueData)
            {
                Console.WriteLine($"ValueData: {valueData}");
            }

            Console.WriteLine("\nWhitelisted Full Entries:");
            foreach (var entry in WhitelistedEntries)
            {
                Console.WriteLine($"Key: {entry.Key}, SubKey: {entry.SubKey}, Value: {entry.Value}, ValueData: {entry.ValueData}");
            }
        }
    }
}

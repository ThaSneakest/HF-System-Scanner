using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Whitelist;

namespace Wildlands_System_Scanner.Scanning
{
    public class FirewallRulesScan
    {
        public static void EnumerateFirewallRules()
        {
            HashSet<string> firewallRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Define registry paths for firewall rules
            string[] registryPaths =
            {
                @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules",
                @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\RestrictedServices\Static\System"
            };

            // Enumerate rules for both 32-bit and 64-bit registry views
            EnumerateRulesFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry64, registryPaths, firewallRules);
            EnumerateRulesFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry32, registryPaths, firewallRules);

            // Process filtered results
            foreach (string rule in firewallRules)
            {
                string ruleName = ExtractRuleName(rule);
                string applicationPath = ExtractApplicationPath(rule);

                // Normalize paths to resolve file redirection
                applicationPath = NormalizeFilePath(applicationPath);

                // Skip logging whitelisted items
                if (IsWhitelisted(ruleName, applicationPath))
                {
                    continue;
                }

                // Mark blacklisted items
                if (IsBlacklisted(ruleName, applicationPath))
                {
                    Logger.Instance.LogPrimary($"{rule} <---- Malicious Entry Found");
                }
                else
                {
                    // Log all other items
                    Logger.Instance.LogPrimary(rule);
                }
            }
        }

        private static void EnumerateRulesFromRegistry(RegistryHive hive, RegistryView view, string[] paths, HashSet<string> firewallRules)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
                {
                    foreach (string path in paths)
                    {
                        using (RegistryKey subKey = baseKey.OpenSubKey(path))
                        {
                            if (subKey == null)
                            {
                                Console.WriteLine($"No firewall rules found in {hive}\\{path} ({view})");
                                continue;
                            }

                            foreach (string valueName in subKey.GetValueNames())
                            {
                                object value = subKey.GetValue(valueName);
                                if (value is string ruleValue)
                                {
                                    string formattedRule = ParseFirewallRule(valueName, ruleValue);
                                    if (!string.IsNullOrEmpty(formattedRule))
                                    {
                                        firewallRules.Add(formattedRule); // Add to HashSet to avoid duplicates
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing registry {hive} ({view}): {ex.Message}");
            }
        }

        private static string ParseFirewallRule(string ruleName, string ruleValue)
        {
            string[] ruleParts = ruleValue.Split('|');
            string action = GetRulePart(ruleParts, "Action");
            string applicationPath = GetRulePart(ruleParts, "App");

            // Normalize the application path to handle redirection
            applicationPath = NormalizeFilePath(applicationPath);

            // Determine if the file exists
            string fileStatus = string.IsNullOrEmpty(applicationPath) || !System.IO.File.Exists(applicationPath) ? "No File" : "Exists";

            // Format the output
            return $"FirewallRules: [{ruleName}] => ({action}) {applicationPath} => {fileStatus}";
        }
        

        private static string GetRulePart(string[] parts, string key)
        {
            foreach (string part in parts)
            {
                if (part.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    return part.Substring(key.Length + 1);
                }
            }
            return null;
        }

        private static string NormalizeFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            // Expand %SystemRoot%
            filePath = filePath.Replace("%SystemRoot%", Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            filePath = Environment.ExpandEnvironmentVariables(filePath);

            // Debug expanded path
            Console.WriteLine($"[DEBUG] Expanded Path: {filePath}");

            // Handle Sysnative redirection for 32-bit processes on 64-bit OS
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                if (filePath.IndexOf(@"\System32\", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string sysnativePath = filePath.Replace(@"\System32\", @"\Sysnative\");
                    if (File.Exists(sysnativePath))
                    {
                        Console.WriteLine($"[DEBUG] Using Sysnative path: {sysnativePath}");
                        return sysnativePath.Replace(@"\Sysnative\", @"\System32\");
                    }
                }
            }

            // Check System32 path directly
            if (File.Exists(filePath))
            {
                return filePath; // File exists in System32
            }

            // Fallback to the original path if nothing works
            return filePath;
        }


        private static string ExtractRuleName(string rule)
        {
            int startIndex = rule.IndexOf("[") + 1;
            int endIndex = rule.IndexOf("]");
            return startIndex > 0 && endIndex > startIndex ? rule.Substring(startIndex, endIndex - startIndex) : null;
        }

        private static string ExtractApplicationPath(string rule)
        {
            int startIndex = rule.IndexOf("=> (") + 4;
            int endIndex = rule.LastIndexOf(" =>");
            return startIndex > 0 && endIndex > startIndex ? rule.Substring(startIndex, endIndex - startIndex).Trim() : null;
        }

        private static bool IsWhitelisted(string ruleName, string applicationPath)
        {
            return FirewallRulesWhitelist.WhitelistFirewallRules.Any(item =>
                string.Equals(item, ruleName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item, applicationPath, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsBlacklisted(string ruleName, string applicationPath)
        {
            return FirewallRulesBlacklist.BlacklistFirewallRules.Any(item =>
                string.Equals(item, ruleName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item, applicationPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class FilterFix
    {
        public static void LUFILDEL1(string keyPath, string valueName, string searchTerm, string logFilePath)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {keyPath}");
                        return;
                    }

                    string data = key.GetValue(valueName) as string;

                    if (string.IsNullOrEmpty(data) || !System.Text.RegularExpressions.Regex.IsMatch(data, $@"(?i)(^|\n){searchTerm}(\n|$)"))
                    {
                        return;
                    }

                    // Remove the specified search term
                    string updatedData = System.Text.RegularExpressions.Regex.Replace(data, $@"(?i)^({searchTerm})(\n|$)|\n({searchTerm})$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
                    updatedData = System.Text.RegularExpressions.Regex.Replace(updatedData, $@"(?i)\n({searchTerm})\n", "\n");

                    if (!string.IsNullOrWhiteSpace(updatedData))
                    {
                        try
                        {
                            // Write the updated REG_MULTI_SZ value
                            key.SetValue(valueName, updatedData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries), RegistryValueKind.MultiString);

                            //Logger.Instance.LogAdditional(logFilePath, $"{keyPath}\\{valueName} {searchTerm} => Value updated and {searchTerm} removed.");
                        }
                        catch (Exception ex)
                        {
                            //Logger.Instance.LogAdditional(logFilePath, $"Failed to update value: {keyPath}\\{valueName} {searchTerm}. Error: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Delete the entire value if no data is left
                        key.DeleteValue(valueName);
                        // Logger.Instance.LogAdditional(logFilePath, $"{keyPath}\\{valueName} {searchTerm} => Value deleted.");
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Instance.LogAdditional(logFilePath, $"Error processing {keyPath}\\{valueName} {searchTerm}: {ex.Message}");
            }
        }

        public static void LUFILFIX(string fix, string system, string bootSystem, string def, double osNum)
        {
            // Extract SID and Filter type using regex replacements
            string sid = System.Text.RegularExpressions.Regex.Replace(fix, @".+\[([^]]+)\] ->.*", "$1");
            string filterType = System.Text.RegularExpressions.Regex.Replace(fix, @"(.+Filters):\s*\[.+", "$1");

            string keyPath = $@"HKLM\{system}\{bootSystem}\{def}\Control\Class\{sid}";

            // Determine the appropriate action
            if (sid == "{4D36E967-E325-11CE-BFC1-08002BE10318}" && filterType == "UpperFilters")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "UpperFilters", "PartMgr", RegistryValueKind.String);
            }
            else if (sid == "{4D36E96B-E325-11CE-BFC1-08002BE10318}" && filterType == "UpperFilters")
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "UpperFilters", "kbdclass", RegistryValueKind.String);
            }
            else if (sid == "{4D36E967-E325-11CE-BFC1-08002BE10318}" && filterType == "LowerFilters" && (osNum == 10 || osNum == 6.3))
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "LowerFilters", "EhStorClass", RegistryValueKind.String);
            }
            else if (sid == "{71A27CDD-812A-11D0-BEC7-08002BE2092F}" && filterType == "UpperFilters" && (osNum == 10 || osNum == 5.1))
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "UpperFilters", "volsnap", RegistryValueKind.String);
            }
            else if (sid == "{71A27CDD-812A-11D0-BEC7-08002BE2092F}" && filterType == "LowerFilters" && osNum == 10)
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "LowerFilters", "fvevol\niorate\nrdyboost", RegistryValueKind.String);
            }
            else if (sid == "{71A27CDD-812A-11D0-BEC7-08002BE2092F}" && filterType == "LowerFilters" && (osNum == 6.3 || osNum == 6.1))
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "LowerFilters", "fvevol\nrdyboost", RegistryValueKind.String);
            }
            else if (sid == "{71A27CDD-812A-11D0-BEC7-08002BE2092F}" && filterType == "LowerFilters" && osNum == 6)
            {
                RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "LowerFilters", "ecache", RegistryValueKind.String);
            }
            else
            {
                RegistryValueHandler.DeleteRegistryValue(keyPath, filterType);
            }
        }

        //Executes the FilterFix logic to remove registry keys based on a specific pattern.
        //The input string containing the filter information.
        public static void FilterFixer(string fixString)
        {
            if (string.IsNullOrEmpty(fixString) || !fixString.Contains("Filter: "))
            {
                return;
            }

            // Extract the subkey
            string subKey = Regex.Replace(fixString, @"Filter: ([^\{]+) - .+ - .*", "$1");

            // Build the registry path for the filter
            string keyPath = $@"HKEY_LOCAL_MACHINE\Software\Classes\PROTOCOLS\Filter\{subKey}";

            // Delete the filter registry key
            RegistryKeyHandler.DeleteRegistryKey(keyPath);

            // Check for a CLSID pattern
            Match clsidMatch = Regex.Match(fixString, @"\{.+\}");
            if (clsidMatch.Success)
            {
                string clsid = clsidMatch.Value;

                // Build the registry path for the CLSID
                string clsidKeyPath = $@"HKEY_LOCAL_MACHINE\Software\Classes\CLSID\{clsid}";

                // Delete the CLSID registry key if it exists
                if (RegistryKeyHandler.RegistryKeyExists(clsidKeyPath))
                {
                    RegistryKeyHandler.DeleteRegistryKey(clsidKeyPath);
                }
            }
        }
    }
}

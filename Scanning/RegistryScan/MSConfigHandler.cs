using DevExpress.Utils.Serializing.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

//Not working
public class MSConfigHandler
{
    private static readonly string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    private static readonly string startupCommonDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

    public static void GetMSConfigData(ref List<string> arr)
    {
        try
        {
            // Define registry keys for MSConfig data
            var keys = new List<(string Key, string Prefix)>
            {
                (@"HKLM\SOFTWARE\Microsoft\Shared Tools\MSConfig\services", "MSCONFIG\\Services: "),
                (@"HKLM\SOFTWARE\Microsoft\Shared Tools\MSConfig\startupfolder", "MSCONFIG\\startupfolder: "),
                (@"HKLM\SOFTWARE\Microsoft\Shared Tools\MSConfig\startupreg", "MSCONFIG\\startupreg: ")
            };

            // Process defined keys
            foreach (var (key, prefix) in keys)
            {
                Console.WriteLine($"Checking key: {key}");
                RegistryUtils.RegRead(key, ref arr, prefix);
            }

            string baseKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved";
            MSCONFIG8(@"HKLM", baseKey, "StartupFolder", ref arr);
            MSCONFIG8(@"HKLM", baseKey, "Run", ref arr);

            // Process user-specific registry keys
            foreach (var userReg in RegistryUserHandler.GetUserRegistryKeys())
            {
                Console.WriteLine($"Processing user registry key: {userReg}");
                if (Regex.IsMatch(userReg, @"S-1-5-\d{2,}-\d{3,}"))
                {
                    string hive = $"HKU\\{userReg}";
                    MSCONFIG8(hive, baseKey, "StartupFolder", ref arr);
                    MSCONFIG8(hive, baseKey, "Run", ref arr);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetMSConfigData: {ex.Message}");
        }
    }

    public static void MSCONFIG8(string hive, string baseKey, string sub, ref List<string> arr)
    {
        try
        {
            string fullKeyPath = $"{hive}\\{baseKey}\\{sub}";
            Console.WriteLine($"Checking subkey: {fullKeyPath}");

            // Use Registry32 for testing on 32-bit systems
            using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey(fullKeyPath))
            {
                if (registryKey == null)
                {
                    Console.WriteLine($"Subkey not found: {fullKeyPath}");
                    return;
                }

                foreach (var valueName in registryKey.GetValueNames())
                {
                    var valueData = registryKey.GetValue(valueName)?.ToString();
                    if (!string.IsNullOrEmpty(valueData) && Regex.IsMatch(valueData, "^(01|03|05|07)"))
                    {
                        Console.WriteLine($"Found matching value: {valueData}");
                        arr.Add($"{hive}\\...\\StartupApproved\\{sub}: => \"{valueName}\"");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MSCONFIG8: {ex.Message}");
        }
    }

    public static List<string> EnumerateMSConfigItems()
    {
        // List to store the disabled items
        List<string> disabledItems = new List<string>();

        // Enumerate disabled startup items from msconfig startupreg
        EnumerateMSConfigItem(@"SOFTWARE\Microsoft\Shared Tools\MSConfig\startupreg", "msconfig - startupreg", disabledItems);

        // Enumerate disabled startup items from msconfig startupfolder
        EnumerateMSConfigItem(@"SOFTWARE\Microsoft\Shared Tools\MSConfig\startupfolder", "msconfig - startupfolder", disabledItems);

        return disabledItems;
    }

    // Helper method to enumerate msconfig items from a specific registry path
    private static void EnumerateMSConfigItem(string registryPath, string description, List<string> disabledItems)
    {
        // Open the registry key
        using (RegistryKey msconfigKey = Registry.LocalMachine.OpenSubKey(registryPath))
        {
            if (msconfigKey != null)
            {
                foreach (string subKeyName in msconfigKey.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey subKey = msconfigKey.OpenSubKey(subKeyName))
                        {
                            if (subKey != null)
                            {
                                string itemName = subKeyName;
                                string itemCommand = subKey.GetValue("command")?.ToString();
                                string itemLocation = subKey.GetValue("location")?.ToString();
                                string itemDateDisabled = subKey.GetValue("date")?.ToString();
                                disabledItems.Add($"Name: {itemName}, Command: {itemCommand}, Location: {itemLocation}, Date Disabled: {itemDateDisabled}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing key {subKeyName}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Failed to open {description} registry key.");
            }
        }
    }

    public static void EnumerateAllDisabledEntries()
    {
        try
        {
            // Log all registry entries for disabled items
            string result = EnumerateDisabledEntriesFromRegistry();
            if (string.IsNullOrWhiteSpace(result))
            {
                Logger.Instance.LogPrimary("No disabled entries found.");
            }
            else
            {
                Logger.Instance.LogPrimary(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during enumeration: {ex.Message}");
        }
    }

    private static string EnumerateDisabledEntriesFromRegistry()
    {
        StringBuilder sb = new StringBuilder();

        // Define registry paths to check
        string[] registryPaths =
        {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32"
    };

        // Root keys to check
        RegistryHive[] rootHives = { RegistryHive.CurrentUser, RegistryHive.LocalMachine };
        RegistryView[] registryViews = { RegistryView.Registry32, RegistryView.Registry64 };

        foreach (RegistryHive rootHive in rootHives)
        {
            foreach (RegistryView view in registryViews)
            {
                foreach (string path in registryPaths)
                {
                    string rootKey = rootHive == RegistryHive.CurrentUser ? "HKU" : "HKLM";

                    try
                    {
                        using (RegistryKey baseKey = RegistryKey.OpenBaseKey(rootHive, view))
                        using (RegistryKey key = baseKey.OpenSubKey(path))
                        {
                            if (key == null)
                            {
                                continue;
                            }

                            foreach (string valueName in key.GetValueNames())
                            {
                                object value = key.GetValue(valueName);
                                byte[] valueData = value as byte[];
                                string state = "Unknown";

                                if (valueData != null && valueData.Length > 0)
                                {
                                    state = valueData[0] == 2 ? "Disabled" : "Enabled";
                                }

                                string registryPath = $"{rootKey}\\{path}";
                                sb.AppendLine($"{registryPath}: => \"{valueName}\" (State: {state})");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Error reading {rootKey}\\{path}: {ex.Message}");
                    }
                }
            }
        }

        return sb.ToString();
    }



}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Whitelist;

namespace Wildlands_System_Scanner.Scanning
{
    public class InstalledProgramsScan
    {
        public static void EnumerateInstalledPrograms()
        {
            List<string> programEntries = new List<string>();

            // Define registry paths to search
            string[] registryPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            // Enumerate programs for both HKEY_LOCAL_MACHINE and HKEY_CURRENT_USER with both 32-bit and 64-bit views
            EnumerateProgramsFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry64, registryPaths, programEntries);
            EnumerateProgramsFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry32, registryPaths, programEntries);
            EnumerateProgramsFromRegistry(RegistryHive.CurrentUser, RegistryView.Registry64, registryPaths, programEntries);
            EnumerateProgramsFromRegistry(RegistryHive.CurrentUser, RegistryView.Registry32, registryPaths, programEntries);

            // Filter entries based on whitelist and blacklist
            var filteredEntries = programEntries
                .Where(entry => !InstalledProgramWhitelist.Whitelist
                    .Any(w => entry.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(entry =>
                {
                    // Mark blacklisted entries as malicious
                    if (InstalledProgramBlacklist.Blacklist.Any(b => entry.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return $"{entry} <---- Malicious Program";
                    }
                    return entry;
                });

            // Log results
            foreach (string entry in filteredEntries)
            {
                Logger.Instance.LogPrimary(entry);
            }
        }

        private static void EnumerateProgramsFromRegistry(RegistryHive hive, RegistryView view, string[] paths, List<string> programEntries)
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
                                continue;

                            foreach (string subKeyName in subKey.GetSubKeyNames())
                            {
                                using (RegistryKey programKey = subKey.OpenSubKey(subKeyName))
                                {
                                    if (programKey == null)
                                        continue;

                                    string displayName = programKey.GetValue("DisplayName") as string;
                                    string version = programKey.GetValue("DisplayVersion") as string;
                                    string publisher = programKey.GetValue("Publisher") as string;

                                    if (string.IsNullOrEmpty(displayName))
                                        continue;

                                    // Hidden or system components detection
                                    bool isHidden = programKey.GetValue("SystemComponent") != null || programKey.GetValue("WindowsInstaller") != null;

                                    // Format output
                                    string formattedEntry = $"{displayName} ({hive}\\{path}\\{subKeyName})";
                                    if (!string.IsNullOrEmpty(version))
                                        formattedEntry += $" (Version: {version})";
                                    if (!string.IsNullOrEmpty(publisher))
                                        formattedEntry += $" - {publisher}";
                                    if (isHidden)
                                        formattedEntry += " Hidden";

                                    programEntries.Add(formattedEntry);
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
    }
}

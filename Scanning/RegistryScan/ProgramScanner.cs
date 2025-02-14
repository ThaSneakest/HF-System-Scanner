using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

public class ProgramScanner
{
    private static readonly string ScanLabel = "Scanning Installed Programs...";
    private static readonly string UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    private static readonly string UserRegistryPathTemplate = @"HKEY_USERS\{0}\Software\Microsoft\Windows\CurrentVersion\Uninstall";


    private static void ScanProgramsFromRegistry(RegistryKey baseKey, string subKeyPath, List<string> installedPrograms, List<string> chromeApps)
    {
        try
        {
            using (RegistryKey uninstallKey = baseKey.OpenSubKey(subKeyPath))
            {
                if (uninstallKey == null) return;

                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    using (RegistryKey subKey = uninstallKey.OpenSubKey(subKeyName))
                    {
                        if (subKey == null) continue;

                        string uninstallString = subKey.GetValue("UninstallString") as string;
                        if (string.IsNullOrEmpty(uninstallString)) continue;

                        string displayName = subKey.GetValue("DisplayName") as string;
                        displayName = displayName?.Trim();

                        if (string.IsNullOrEmpty(displayName) ||
                            displayName.StartsWith("Security Update for", StringComparison.OrdinalIgnoreCase) ||
                            displayName.StartsWith("Hotfix for", StringComparison.OrdinalIgnoreCase) ||
                            displayName.StartsWith("Update for Microsoft", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string publisher = subKey.GetValue("Publisher") as string;
                        string version = subKey.GetValue("DisplayVersion") as string;
                        bool isHidden = (subKey.GetValue("SystemComponent") as int?) == 1;

                        string entry = $"{displayName} (Version: {version ?? "Unknown"} - {publisher ?? "Unknown"})";
                        if (isHidden) entry += " [Hidden]";

                        if (!string.IsNullOrEmpty(publisher) && publisher.Contains("Google\\Chrome"))
                        {
                            chromeApps.Add(entry);
                        }
                        else
                        {
                            installedPrograms.Add(entry);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning registry: {ex.Message}");
        }
    }

    private static void WriteProgramsToOutput(List<string> programs, string sectionTitle)
    {
        if (programs.Count == 0) return;

        Console.WriteLine();
        Console.WriteLine($"==================== {sectionTitle} ======================");
        Console.WriteLine();

        programs = programs.Distinct().OrderBy(p => p).ToList();
        foreach (var program in programs)
        {
            Console.WriteLine(program);
        }
    }
}

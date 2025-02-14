using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public class BHOHandler
{
    // Whitelist: (KeyPath, ValueName, ValueData)
    private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", "{GUID}", "SafeBHO.dll"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", "{GUID}", "SafeBHO.dll"),
            Tuple.Create(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", "{GUID}", "SafeBHO.dll"),
        };

    // Blacklist: (KeyPath, ValueName, ValueData)
    private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", "{MaliciousGUID}", "MaliciousBHO.dll"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", "{MaliciousGUID}", "MaliciousBHO.dll"),
            Tuple.Create(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects", "{MaliciousGUID}", "MaliciousBHO.dll"),
        };

    public static void ScanBrowserHelperObjects()
    {
        string[] registryPaths = new[]
        {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects",
                @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects"
            };

        ScanRegistryPaths(Registry.LocalMachine, registryPaths);
        ScanRegistryPaths(Registry.CurrentUser, new[] { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects" });
    }

    private static void ScanRegistryPaths(RegistryKey baseRegistry, string[] registryPaths)
    {
        foreach (string path in registryPaths)
        {
            ScanKey(baseRegistry, path);
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


using DevExpress.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

//Tested and working 

public class Drivers32Handler
{
    // Define registry paths to scan
    private static readonly string[] RegistryPaths =
    {
            @"Software\Microsoft\Windows NT\CurrentVersion\Drivers32",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Drivers32",
            @"Software\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Drivers32",
            @"Software\Microsoft\Windows NT\CurrentVersion\Drivers32",
            @"Software\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Drivers32"
        };

    // Whitelist: (KeyPath, ValueName, ValueData)
    private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Drivers32", "wave", "wdmaud.drv"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Drivers32", "midi", "wdmaud.drv"),
        };

    // Blacklist: (KeyPath, ValueName, ValueData)
    private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Drivers32", "midi", "malicious.dll"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Drivers32", "wave", "hacked_driver.sys"),
        };

    public static void ScanDrivers32()
    {
        Console.WriteLine("Starting scan for Drivers32 registry paths...");

        // Scan HKLM and HKCU
        foreach (string path in RegistryPaths)
        {
            ScanKey(Registry.LocalMachine, path);
            ScanKey(Registry.CurrentUser, path);
        }

        // Scan HKU for all user keys
        using (RegistryKey hkuRoot = Registry.Users)
        {
            foreach (string userKey in hkuRoot.GetSubKeyNames())
            {
                string userPath = $@"{userKey}\Software\Microsoft\Windows NT\CurrentVersion\Drivers32";
                ScanKey(hkuRoot, userPath);
            }
        }

        Console.WriteLine("Scan complete.");
    }

    private static void ScanKey(RegistryKey baseRegistry, string subPath)
    {
        string fullKeyPath = $@"{baseRegistry.Name}\{subPath}";
        Console.WriteLine($"Scanning: {fullKeyPath}");

        try
        {
            using (RegistryKey baseKey = baseRegistry.OpenSubKey(subPath, false)) // Read-only mode
            {
                if (baseKey == null)
                {
                    Console.WriteLine($"Registry key not found: {fullKeyPath}");
                    return;
                }

                Console.WriteLine($"Opened registry key: {fullKeyPath}");

                // Enumerate values
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

                // Enumerate subkeys recursively
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

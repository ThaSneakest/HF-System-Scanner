using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public static class LsaHandler
{
    private static readonly string RegistryPath = @"System\CurrentControlSet\Control\Lsa";

    // Whitelist: (KeyPath, ValueName, ValueData)
    private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa", "Authentication Packages", "msv1_0"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa", "Security Packages", "kerberos"),
        };

    // Blacklist: (KeyPath, ValueName, ValueData)
    private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa", "Authentication Packages", "malicious_auth.dll"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Lsa", "Security Packages", "evil_kerberos"),
        };

    public static void ScanLsaRegistry()
    {
        Console.WriteLine("Starting scan for LSA registry key...");

        ScanKey(Registry.LocalMachine, RegistryPath, "HKEY_LOCAL_MACHINE");

        Console.WriteLine("Scan complete.");
    }

    private static void ScanKey(RegistryKey baseRegistry, string subPath, string rootName)
    {
        string fullKeyPath = $@"{rootName}\{subPath}";
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
                    ScanKey(baseRegistry, subKeyFullPath, rootName);
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

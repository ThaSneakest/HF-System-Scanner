using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

//Working and tested needs whitelisted and worked on
public class PolicyHandler
{
    private static readonly string[] RegistryPaths = new[]
    {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
            @"SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore",
            @"SOFTWARE\Policies\Microsoft\Windows Defender",
            @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate",
            @"SOFTWARE\Policies\Microsoft\MRT",
            @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center",
            @"SOFTWARE\Policies\Microsoft\WindowsFirewall",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
            @"SOFTWARE\Policies\Microsoft\MpEngine",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\Shell",
            @"SOFTWARE\Microsoft\Windows Defender",
            @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy",
            @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile",
            @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options",
            @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Image File Execution Options"
        };

    private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender", "DisableAntiSpyware", "0"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender", "DisableAntiVirus", "0"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR", "0"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Policies\System", "EnableLUA", "1"),
        };

    private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender", "DisableAntiSpyware", "1"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender", "DisableAntiVirus", "1"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Policies\System", "EnableLUA", "0"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableSR", "1"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "DisableRegistryTools", "1"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableTaskMgr", "1"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableCMD", "1"),
        };

    public static void ScanRegistry()
    {
        Console.WriteLine("Starting scan for registry policies...");

        foreach (string path in RegistryPaths)
        {
            ScanKey(Registry.LocalMachine, path, "HKEY_LOCAL_MACHINE");
            ScanKey(Registry.CurrentUser, path, "HKEY_CURRENT_USER");
        }

        ScanUserKeys();

        Console.WriteLine("Scan complete.");
    }

    private static void ScanUserKeys()
    {
        using (RegistryKey usersKey = Registry.Users)
        {
            foreach (string userSID in usersKey.GetSubKeyNames())
            {
                ScanKey(Registry.Users, $@"{userSID}\Software\Microsoft\Windows\CurrentVersion\Policies\System", $"HKEY_USERS\\{userSID}");
                ScanKey(Registry.Users, $@"{userSID}\Control Panel\Desktop", $"HKEY_USERS\\{userSID}");
                ScanKey(Registry.Users, $@"{userSID}\Software\Microsoft\Windows Defender", $"HKEY_USERS\\{userSID}");
            }
        }
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

                foreach (string valueName in baseKey.GetValueNames())
                {
                    string valueData = baseKey.GetValue(valueName)?.ToString() ?? "NULL";

                    if (Whitelist.Contains(Tuple.Create(fullKeyPath, valueName, valueData)))
                    {
                        Console.WriteLine($"Whitelisted entry skipped: {fullKeyPath}: [{valueName}] -> {valueData}");
                        continue;
                    }

                    string attn = Blacklist.Contains(Tuple.Create(fullKeyPath, valueName, valueData))
                        ? " <==== Malicious Registry Entry Found"
                        : string.Empty;

                    Logger.Instance.LogPrimary($"{fullKeyPath}: [{valueName}] -> {valueData}{attn}");
                }

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

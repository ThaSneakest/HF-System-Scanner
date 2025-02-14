using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.Win32;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

public class ActiveSetupHandler
{
    // Registry Key Whitelist
    private static readonly HashSet<string> RegistryKeyWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        @"HKEY_LOCAL_MACHINE\Software\Microsoft\Active Setup\Installed Components",
        @"HKEY_LOCAL_MACHINE\Software\WOW6432Node\Microsoft\Active Setup\Installed Components",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\>{22d6f312-b0f6-11d0-94ab-0080c74c7e95}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{052860C8-3E53-3D0B-9332-48A8B4971352}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{22d6f312-b0f6-11d0-94ab-0080c74c7e95}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{2C7339CF-2B09-4501-B3F3-F3508C9228ED}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{3af36230-a269-11d1-b5bf-0000f8051515}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{44BBA855-CC51-11CF-AAFA-00AA00B6015F}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{45ea75a0-a269-11d1-b5bf-0000f8051515}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{4f645220-306d-11d2-995d-00c04f98bbc9}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{5fd399c0-a70a-11d1-9948-00c04f98bbc9}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{630b1da0-b465-11d1-9948-00c04f98bbc9}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{6AE338E1-C149-37E8-A9F5-5BD4E029E1D8}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{6BF52A52-394A-11d3-B153-00C04F79FAA6}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{6fab99d0-bab8-11d1-994a-00c04f98bbc9}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{7790769C-0471-11d2-AF11-00C04FA35D02}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{89820200-ECBD-11cf-8B85-00AA005B4340}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{89820200-ECBD-11cf-8B85-00AA005B4383}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{89B4C1CD-B018-4511-B0A1-5476DBF70820}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{9381D8F2-0288-11D0-9501-00AA00B911A5}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{9459C573-B17A-45AE-9F64-1857B5D58CEE}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{C9E9A340-D1F1-11D0-821E-444553540600}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{de5aed00-a4bf-11d1-9948-00c04f98bbc9}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{E92B03AB-B707-11d2-9CBD-0000F87A369E}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{FEBEF00C-046D-438D-8A88-BF94A6C9E703}",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Active Setup\Installed Components\{2C7339CF-2B09-4501-B3F3-F3508C9228ED}",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Active Setup\Installed Components\{6BF52A52-394A-11d3-B153-00C04F79FAA6}",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Active Setup\Installed Components\{89820200-ECBD-11cf-8B85-00AA005B4340}",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Active Setup\Installed Components\{89820200-ECBD-11cf-8B85-00AA005B4383}",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Active Setup\Installed Components\{89B4C1CD-B018-4511-B0A1-5476DBF70820}",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Active Setup\Installed Components\{8A69D345-D564-463c-AFF1-A69D9E530F96}",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Active Setup\Installed Components\{9459C573-B17A-45AE-9F64-1857B5D58CEE}"
    };

    // Registry Key, Value, and Data Whitelist
    private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>()
    {
        Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Active Setup\Installed Components", "StubPath", @"C:\Windows\System32\stub.exe"),
        Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Active Setup\Installed Components", "StubPath", @"C:\Program Files\Common Files\example.exe"),
    };

    // Registry Key Blacklist
    private static readonly HashSet<string> RegistryKeyBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{FD1CEC34-A56C-BB1B-F3BF-657ECFEBE6EE}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{82DMO5S0-8SBR-43X5-M287-5B02P320F0E0}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{933BDABB-4B27-ECF3-6EB4-F3E68821E933}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\ô9h€",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\{2E2CA91C-CEBB-C6B1-0007-060203030507}",
        @"HKEY_LOCAL_MACHINE\Software\Microsoft\Active Setup\Installed Components\{08B0E5JF-4FCB-11CF-AAA5-00401C6XX500}",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\Explorer.exe",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\victims121",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\smcss",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\StubPath"

    };

    // Registry Key, Value, and Data Blacklist
    private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>()
    {
        Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components\StubPath", "StubPath", "Test"),
        Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\WOW6432Node\Microsoft\Active Setup\Installed Components", "ShellComponent", "malicious.dll"),
    };

    public static void HandleActiveSetup(bool isCheckEditEnabled)
    {
        string[] keyPaths =
        {
            @"Software\Microsoft\Active Setup\Installed Components",
            @"Software\WOW6432Node\Microsoft\Active Setup\Installed Components",
            @"HKCU\Software\Microsoft\Active Setup\Installed Components"
        };

        try
        {
            foreach (var rootKeyPath in keyPaths)
            {
                RegistryKey rootKey = rootKeyPath.StartsWith("HKCU")
                    ? Registry.CurrentUser
                    : Registry.LocalMachine;

                string keyPath = rootKeyPath.StartsWith("HKCU")
                    ? rootKeyPath.Replace("HKCU\\", string.Empty)
                    : rootKeyPath.Replace("HKLM\\", string.Empty);

                using (RegistryKey baseKey = rootKey.OpenSubKey(keyPath))
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {rootKeyPath}");
                        continue;
                    }

                    int index = 0;
                    while (true)
                    {
                        string subKeyName = RegistrySubKeyHandler.EnumerateSubKey(baseKey, index++);
                        if (subKeyName == null)
                            break;

                        string fullSubKeyPath = $"{rootKeyPath}\\{subKeyName}";

                        if (RegistryKeyWhitelist.Contains(fullSubKeyPath))
                            continue;

                        using (RegistryKey subKey = baseKey.OpenSubKey(subKeyName))
                        {
                            if (subKey == null)
                                continue;

                            string data = RegistryValueHandler.TryReadRegistryValue(subKey, "StubPath");
                            if (string.IsNullOrEmpty(data))
                                continue;

                            if (Whitelist.Contains(Tuple.Create(fullSubKeyPath, "StubPath", data)))
                                continue;

                            string cdate = "Invalid file";
                            string company = "Unknown company";
                            string file = data;

                            string attn = string.Empty;
                            if (RegistryKeyBlacklist.Contains(fullSubKeyPath) ||
                                Blacklist.Contains(Tuple.Create(fullSubKeyPath, "StubPath", data)))
                            {
                                attn = " <==== Malicious Registry Entry";
                            }

                            Logger.Instance.LogPrimary($"{fullSubKeyPath}: [{subKeyName}] -> {file} {cdate} {company} {attn}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while handling Active Setup: {ex.Message}");
        }
    }
}
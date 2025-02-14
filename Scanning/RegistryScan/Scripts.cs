using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class Scripts
    {
        private static readonly string[] RegistryPaths = new[]
{
            @"Software\Policies\Microsoft\Windows\System\Scripts",
            @"Software\Policies\Microsoft\Windows\System\Scripts\Startup",
            @"Software\Policies\Microsoft\Windows\System\Scripts\Logon",
            @"Software\Policies\Microsoft\Windows\System\Scripts\Logoff",
            @"Software\Policies\Microsoft\Windows\System\Scripts\Shutdown",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Shutdown",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logoff",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logon",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logon",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Logoff",
            @"Software\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Shutdown",
            @"Software\Policies\Microsoft\Windows\System\Scripts\Logon",
            @"Software\Policies\Microsoft\Windows\System\Scripts\Logoff",
        };

        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\System\Scripts\Startup", "Script", "SafeScript.bat"),
        };

        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\System\Scripts\Startup", "Script", "MaliciousScript.bat"),
        };

        public static void ScanGroupPolicyScripts()
        {
            Console.WriteLine($"Starting scan for Group Policy Scripts...");

            ScanRegistryRoot(Microsoft.Win32.Registry.LocalMachine, "HKEY_LOCAL_MACHINE");
            ScanRegistryRoot(Microsoft.Win32.Registry.CurrentUser, "HKEY_CURRENT_USER");
        }

        private static void ScanRegistryRoot(RegistryKey rootKey, string rootName)
        {
            foreach (var path in RegistryPaths)
            {
                ScanKey(rootKey, path, rootName);
            }
        }

        private static void ScanKey(RegistryKey baseRegistry, string subPath, string rootName)
        {
            string fullKeyPath = $@"{rootName}\{subPath}";

            try
            {
                using (RegistryKey baseKey = baseRegistry.OpenSubKey(subPath, false))
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine($"Registry key not found or inaccessible: {fullKeyPath}");
                        return;
                    }

                    foreach (string valueName in baseKey.GetValueNames())
                    {
                        string valueData = baseKey.GetValue(valueName)?.ToString() ?? "NULL";

                        if (Whitelist.Contains(Tuple.Create(fullKeyPath, valueName, valueData)))
                            continue;

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
                Console.WriteLine($"Access denied: {fullKeyPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning {fullKeyPath}: {ex.Message}");
            }
        }
    }
}

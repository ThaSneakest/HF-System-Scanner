using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class ShellExecuteHooks
    {
        private static readonly string[] RegistryPaths = new[]
{
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks",
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks",
            @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks"
        };

        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks", "{AEB6717E-7E19-11d0-97EE-00C04FD91972}", "SafeShellHook.dll"),
        };

        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Explorer\ShellExecuteHooks", "{B7CDF620-DB73-44C0-8611-832B261A0107}", "MaliciousShellHook.dll"),
        };

        public static void ScanShellExecuteHooks()
        {
            Console.WriteLine($"Starting scan for ShellExecuteHooks registry keys...");

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

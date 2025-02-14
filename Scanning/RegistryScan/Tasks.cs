using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class Tasks
    {
        private static readonly string[] RegistryPaths = new[]
    {
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\SharedTaskScheduler",
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\SharedTaskScheduler",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Tasks"
        };

        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache", "", ""),
        };

        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks", "TaskName", "MaliciousTask"),
        };

        public static void ScanTaskScheduler()
        {
            Console.WriteLine($"Starting scan for Task Scheduler registry keys...");

            try
            {
                foreach (var path in RegistryPaths)
                {
                    ScanKey(Microsoft.Win32.Registry.LocalMachine, path, "HKEY_LOCAL_MACHINE");
                }

                // Scan HKU SIDs
                using (RegistryKey usersKey = Microsoft.Win32.Registry.Users)
                {
                    foreach (var sid in usersKey.GetSubKeyNames())
                    {
                        string userPath = $@"{sid}\Software\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks";
                        ScanKey(Microsoft.Win32.Registry.Users, userPath, "HKEY_USERS");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while scanning: {ex.Message}");
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

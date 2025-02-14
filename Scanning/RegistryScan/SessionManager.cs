using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class SessionManager
    {
        private static readonly string[] RegistryPaths = new[]
 {
            @"SYSTEM\ControlSet001\Control\Session Manager",
            @"SYSTEM\ControlSet002\Control\Session Manager\BootExecute",
            @"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs",
            @"SYSTEM\ControlSet002\Control\Session Manager",
            @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment",
            @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters",
            @"System\CurrentControlSet\Control\Session Manager",
            @"System\CurrentControlSet\Control\Session Manager\AppCertDlls",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Control\Session Manager\KnownDLLs",
            @"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs",
            @"System\CurrentControlSet\Control\Session Manager\BootExecute",
            @"System\CurrentControlSet\Control\Session Manager\SetupExecute",
            @"System\CurrentControlSet\Control\Session Manager\Execute",
            @"System\CurrentControlSet\Control\Session Manager\S0InitialCommand",
            @"Software\Microsoft\Windows NT\CurrentVersion\Control\Session Manager\AppCertDlls"
        };

        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "COR_PROFILER", ""),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "COR_PROFILER_PATH", ""),
        };

        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "COR_PROFILER", "MaliciousProfiler"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\AppCertDlls", "SuspiciousDll", "C:\\Malware\\bad.dll"),
        };

        public static void ScanSessionManager()
        {
            Console.WriteLine($"Starting scan for Session Manager registry keys...");

            ScanRegistryRoot(Microsoft.Win32.Registry.LocalMachine, "HKEY_LOCAL_MACHINE");
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

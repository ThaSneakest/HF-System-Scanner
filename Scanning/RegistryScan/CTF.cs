using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class CTF
    {
        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Ctf\LangBarAddin", "DefaultValue", "SafeData"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\Ctf\LangBarAddin", "DefaultValue", "SafeData"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Ctf\LangBarAddin", "DefaultValue", "SafeData"),
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Ctf\LangBarAddin", "MaliciousEntry", "MaliciousData"),
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Microsoft\Ctf\LangBarAddin", "MaliciousEntry", "MaliciousData"),
            Tuple.Create(@"HKEY_CURRENT_USER\Software\Microsoft\Ctf\LangBarAddin", "MaliciousEntry", "MaliciousData"),
        };

        public static void ScanCtfLangBarAddinRegistry()
        {
            string[] registryPaths = new[]
            {
                @"Software\Microsoft\Ctf\LangBarAddin",
                @"Software\Wow6432Node\Microsoft\Ctf\LangBarAddin"
            };

            // Scan HKLM
            foreach (string path in registryPaths)
            {
                ScanKey(Microsoft.Win32.Registry.LocalMachine, path);
            }

            // Scan HKCU
            ScanKey(Microsoft.Win32.Registry.CurrentUser, @"Software\Microsoft\Ctf\LangBarAddin");
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

}

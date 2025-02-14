using System;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.Registry;
using System.Collections.Generic;

//Tested and working
public class CertificateHandler
{
        // Whitelist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SystemCertificates\Disallowed\Certificates", "SafeCert", "SafeData"),
        };

        // Blacklist: (KeyPath, ValueName, ValueData)
        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SystemCertificates\Disallowed\Certificates", "MaliciousCert", "MaliciousData"),
        };

        public static void ScanDisallowedCertificatesRegistry()
        {
            string registryPath = @"SOFTWARE\Microsoft\SystemCertificates\Disallowed\Certificates";
            Console.WriteLine($"Scanning registry path: HKLM\\{registryPath}");

            try
            {
                ScanKey(Registry.LocalMachine, registryPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while scanning: {ex.Message}");
            }
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

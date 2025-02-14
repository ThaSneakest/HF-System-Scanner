﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scanning.RegistryScan
{
    public class ShellExtensions
    {
        private static readonly string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved";

        private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved", "{E357FCCD-A995-4576-B01F-234630154E96}", "Safe Extension"),
        };

        private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved", "{12345678-ABCD-1234-ABCD-1234567890AB}", "Malicious Extension"),
        };

        public static void ScanShellExtensions()
        {
            Console.WriteLine($"Starting scan for Shell Extensions Approved registry keys...");

            ScanRegistryRoot(Microsoft.Win32.Registry.LocalMachine, "HKEY_LOCAL_MACHINE");
        }

        private static void ScanRegistryRoot(RegistryKey rootKey, string rootName)
        {
            ScanKey(rootKey, RegistryPath, rootName);
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

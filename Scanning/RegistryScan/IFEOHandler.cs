using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

//Tested and working

namespace Wildlands_System_Scanner
{
    public class IFEOHandler
    {
            private static readonly string[] RegistryPaths = new[]
            {
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options",
            @"Software\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Image File Execution Options"
        };

            // Whitelist: (KeyPath, ValueName, ValueData)
            private static readonly HashSet<Tuple<string, string, string>> Whitelist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options", "Debugger", @"C:\Safe\debugger.exe"),
        };

            // Blacklist: (KeyPath, ValueName, ValueData)
            private static readonly HashSet<Tuple<string, string, string>> Blacklist = new HashSet<Tuple<string, string, string>>(new TupleStringComparer())
        {
            Tuple.Create(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options", "Debugger", @"C:\Malicious\malware.exe"),
            Tuple.Create(@"HKEY_USERS\{SID}\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options", "Debugger", @"C:\Trojan\debugger.exe"),
        };

            public static void ScanImageFileExecutionOptions()
            {
                Console.WriteLine("Starting scan for Image File Execution Options...");

                // Scan HKLM paths
                foreach (var path in RegistryPaths)
                {
                    ScanKey(Microsoft.Win32.Registry.LocalMachine, path, "HKEY_LOCAL_MACHINE");
                }

                // Scan HKU for all user SIDs
                ScanUserHives();

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

            private static void ScanUserHives()
            {
                using (RegistryKey usersKey = Microsoft.Win32.Registry.Users)
                {
                    foreach (string userSid in usersKey.GetSubKeyNames())
                    {
                        string userPath = $@"{userSid}\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
                        ScanKey(Microsoft.Win32.Registry.Users, userPath, "HKEY_USERS");
                    }
                }
            }
        }
    }
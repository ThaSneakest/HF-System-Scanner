using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner
{
    public class EnvironmentHandler
    {
        private static void HandleEnvironment(string userKey)
        {
            string environmentSubKey = $@"{userKey}\Environment";
            Console.WriteLine($"Scanning Registry: HKU\\{environmentSubKey}");

            // Open the HKU root key first
            using (RegistryKey hkuKey = Microsoft.Win32.Registry.Users.OpenSubKey(environmentSubKey))
            {
                if (hkuKey != null)
                {
                    foreach (var valueName in hkuKey.GetValueNames())
                    {
                        string valueData = hkuKey.GetValue(valueName)?.ToString();
                        Console.WriteLine($"Environment: [{valueName}] => {valueData}");
                    }
                }
                else
                {
                    Console.WriteLine($"Key HKU\\{environmentSubKey} not found.");
                }
            }
        }

        private static void ScanEnvironmentKeys(string keyPath, bool addToArray, List<string> registryEntries)
        {
            Console.WriteLine($"Scanning Registry: {keyPath}");

            // Open the registry key under HKEY_USERS
            using (var key = Microsoft.Win32.Registry.Users.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var valueData = key.GetValue(valueName)?.ToString();

                        // Check if the value matches the pattern
                        if (valueData != null &&
                            System.Text.RegularExpressions.Regex.IsMatch(valueData, @"\bcmd\b.+\b\.(exe|dll|sys)\b"))
                        {
                            if (addToArray)
                            {
                                // Add the entry to the list
                                ArrayUtils.AddToArrayRegistry(registryEntries, keyPath, valueName, valueData,
                                    "Potential Issue");
                            }
                            else
                            {
                                // Print to the console if not adding to array
                                Console.WriteLine($"{keyPath}: [{valueName}] => {valueData} <==== Potential Issue");
                            }
                        }
                    }
                }
            }
        }

        private static void ScanEnvironmentVariables()
        {
            List<string> registryEntries = new List<string>();

            // Call ScanEnvironmentKey with the appropriate arguments
            ScanEnvironmentKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "COR_PROFILER", registryEntries);
            ScanEnvironmentKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "COR_PROFILER_PATH", registryEntries);

        }

        private static void ScanEnvironmentKey(string keyPath, string valueName, List<string> registryEntries)
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    var valueData = key.GetValue(valueName)?.ToString();
                    if (!string.IsNullOrEmpty(valueData))
                    {
                        // Add the entry to the registryEntries list
                        AddToRegistryEntries(registryEntries, $"Environment Key: {keyPath}, ValueName: {valueName}, ValueData: {valueData}");
                    }
                }
            }
        }
        public static void AddToRegistryEntries(List<string> registryEntries, string registryEntry)
        {
            // Add the registry entry string to the list
            registryEntries.Add(registryEntry);
        }
    }
}

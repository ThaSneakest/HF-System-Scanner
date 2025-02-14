using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Backup;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner.Registry
{
    public class RegistryValueHandler
    {
        private const uint STATUS_SUCCESS = 0x00000000;
        private const uint NTSTATUS_NO_MORE_ENTRIES = 0xC0000034;

        public static string TryReadRegistryValue(RegistryKey key, string valueName)
        {
            try
            {
                object value = key.GetValue(valueName);
                Console.WriteLine($"Reading value '{valueName}' from key '{key.Name}'");

                return value?.ToString() ?? string.Empty;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry value '{valueName}': {ex.Message}");
                return string.Empty;
            }
        }

        public static bool DeleteRegistryValueBool(string keyPath, string valueName)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key != null)
                    {
                        if (key.GetValue(valueName) != null)
                        {
                            key.DeleteValue(valueName);
                            return true; // Successfully deleted
                        }
                    }
                }
                return false; // Value does not exist
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry value: {ex.Message}");
                return false; // Failure
            }
        }

        public static string TryReadRegistryValue(RegistryKey baseKey, string subKeyPath, string valueName)
        {
            try
            {
                using (var subKey = baseKey.OpenSubKey(subKeyPath))
                {
                    if (subKey == null)
                    {
                        Console.WriteLine($"SubKey not found: {subKeyPath}");
                        return string.Empty;
                    }

                    object value = subKey.GetValue(valueName);
                    return value?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry value '{valueName}' in '{subKeyPath}': {ex.Message}");
                return string.Empty;
            }
        }


        public static void DeleteRegistryValue(string key, string value)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
                {
                    if (registryKey != null)
                    {
                        registryKey.DeleteValue(value, throwOnMissingValue: false);
                        Console.WriteLine($"Deleted value: {value} from {key}");
                    }
                    else
                    {
                        Console.WriteLine($"Registry key not found: {key}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry value: {ex.Message}");
            }
        }


        public static List<KeyValuePair<string, string>> GetRegistryValuesAsList(string registryPath)
        {
            List<KeyValuePair<string, string>> registryValues = new List<KeyValuePair<string, string>>();

            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key == null) return null;

                foreach (string valueName in key.GetValueNames())
                {
                    string valueData = key.GetValue(valueName)?.ToString();
                    if (!string.IsNullOrEmpty(valueData))
                    {
                        registryValues.Add(new KeyValuePair<string, string>(valueName, valueData));
                    }
                }
            }

            return registryValues;
        }


        // Method to write a registry value
        public static void WriteRegistryValue(string key, string valueName, string value)
        {
            try
            {
                using (var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
                {
                    if (registryKey != null)
                    {
                        registryKey.SetValue(valueName, value, RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing registry value: {ex.Message}");
            }
        }

        // Method to read a registry value
        public static string ReadRegistryValue(string key, string valueName, string defaultValue = null)
        {
            try
            {
                using (var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {key}");
                        return defaultValue;
                    }

                    var value = registryKey.GetValue(valueName);
                    return value?.ToString() ?? defaultValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry value '{valueName}' from '{key}': {ex.Message}");
                return defaultValue;
            }
        }


        public static void RestoreRegistryValue(string keyPath, string valueName, string valueData)
        {
            try
            {
                // Open the registry key for writing
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key != null)
                    {
                        // Set the value for the "AlternateShell" registry key
                        key.SetValue(valueName, valueData, RegistryValueKind.String);
                        Console.WriteLine($"Restored {valueName} to {valueData} in {keyPath}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to open registry key: {keyPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while restoring registry value: {ex.Message}");
            }
        }


        // Helper method to get registry value by index
        public static string GetRegistryValueByIndex(RegistryKey hkey, int index)
        {
            try
            {
                string[] valueNames = hkey.GetValueNames();
                return valueNames.Length > index ? valueNames[index] : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting registry value at index {index}: {ex.Message}");
                return null;
            }
        }

        public static long ListRegistryValues(IntPtr hKey)
        {
            Structs.TAGKEY_VALUE_PARTIAL_INFORMATION svi = new Structs.TAGKEY_VALUE_PARTIAL_INFORMATION();
            uint index = 0;
            uint resultLength = 0;
            long returnCode = 0; // This will be used to return status as a long value

            List<string[]> entries = new List<string[]>();

            while (true)
            {
                int ret = NtdllNativeMethods.NtEnumerateValueKey(hKey, index, 2, Marshal.UnsafeAddrOfPinnedArrayElement(new byte[Marshal.SizeOf(typeof(Structs.TAGKEY_VALUE_PARTIAL_INFORMATION))], 0), (uint)Marshal.SizeOf(typeof(Structs.TAGKEY_VALUE_PARTIAL_INFORMATION)), ref resultLength);

                if (ret != STATUS_SUCCESS && ret != NTSTATUS_NO_MORE_ENTRIES)
                {
                    if (ret == 0xC0000008)  // Status error: Invalid handle
                        break;

                    index++;
                    continue;
                }

                // Handle status success (value found)
                if (ret == STATUS_SUCCESS)
                {
                    // Get the type and data from the structure
                    uint type = svi.Type;
                    if (type == 6)  // REG_LINK type
                    {
                        uint dataLength = svi.DataLength;
                        IntPtr data = svi.Data;
                        string dataValue = Marshal.PtrToStringUni(data); // Convert pointer to string
                        entries.Add(new string[] { "RegLink", dataValue });
                        returnCode = 1;  // Successfully found a registry link
                    }
                }

                index++;

                if (ret == NTSTATUS_NO_MORE_ENTRIES)
                    break;
            }

            // If entries were found, return 1. Otherwise, return 0 (or another status code)
            if (entries.Count > 0)
            {
                // For example, you can log the entries or do something with them.
                Console.WriteLine("Found " + entries.Count + " registry links.");
            }

            return returnCode;  // Returning a long indicating the status or the count
        }


        public static void RestoreRegistryValue(RegistryHive hive, string keyPath, string valueName, string valueData,
            RegistryValueKind valueKind)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (RegistryKey key = baseKey.CreateSubKey(keyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, valueData, valueKind);
                        Console.WriteLine($"Registry value restored: [{keyPath}] {valueName} = {valueData}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to open or create registry key: {keyPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring registry value: {ex.Message}");
            }
        }


        // Placeholder method to simulate _LISTVAL in AutoIt, which lists the values for a registry key
        public static List<List<string>> ListRegistryValues(RegistryKey hKey)
        {
            List<List<string>> list = new List<List<string>>();
            foreach (string valueName in hKey.GetValueNames())
            {
                string valueData = hKey.GetValue(valueName)?.ToString();
                list.Add(new List<string> { valueName, valueData });
            }

            return list;
        }


        public static void SetRegistryValue(string keyPath, string valueName, string valueData)
        {
            try
            {
                // Open the registry key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true);
                if (key == null)
                {
                    Console.WriteLine("Registry key not found.");
                    return;
                }

                // Set the registry value
                key.SetValue(valueName, valueData, RegistryValueKind.ExpandString);
                Console.WriteLine($"Registry value set: {keyPath}\\{valueName} = {valueData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting registry value: {ex.Message}");
            }
        }

        public static string EnumerateRegistryValue(string key, int index)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        string[] valueNames = registryKey.GetValueNames();
                        if (index - 1 < valueNames.Length)
                            return valueNames[index - 1]; // Return the value name at the specified index
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry value: {ex.Message}");
            }

            return string.Empty;
        }


        public static void HandleRegistryValue(string userKey, string subKey, string valueName, string valueData)
        {
            if (string.IsNullOrEmpty(valueData)) return;

            string attention = "";
            if (valueName.Contains("<*>"))
            {
                // Handle specific cases for certain keys
                attention = " <==== Requires attention";
            }

            Console.WriteLine($"Registry Key: HKU\\{userKey}\\{subKey}: [{valueName}] => {valueData}{attention}");
        }

        public static string EnumerateRegistryValue(RegistryKey key, int index)
        {
            try
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                // Get all value names under the provided key
                string[] valueNames = key.GetValueNames();

                if (index < 0 || index >= valueNames.Length)
                {
                    throw new IndexOutOfRangeException("Invalid index for registry value enumeration.");
                }

                // Get the value name and then retrieve its data
                string valueName = valueNames[index];
                object valueData = key.GetValue(valueName);

                // Return the value as a string
                return valueData?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Handle exceptions or return a fallback value
                Console.WriteLine($"Error enumerating registry value at index {index}: {ex.Message}");
                return string.Empty;
            }
        }


    }
}


using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Registry
{
    public class RegistryKeyHandler
    {

        public static void DeleteRegistryKey(string key)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
                {
                    if (registryKey != null)
                    {
                        registryKey.DeleteSubKeyTree(key); // This will delete the subkey and its children
                        Console.WriteLine($"Registry key '{key}' deleted successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key '{key}': {ex.Message}");
            }
        }

        // Simulating the Registry open function
        public static RegistryKey OpenRegistryKey(string key)
        {
            try
            {
                return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key);
            }
            catch
            {
                return null;
            }
        }

        public static Dictionary<string, Dictionary<string, object>> EnumerateRegistryKey(RegistryKey rootKey, string subKeyPath)
        {
            var results = new Dictionary<string, Dictionary<string, object>>();

            try
            {
                using (var subKey = rootKey.OpenSubKey(subKeyPath))
                {
                    if (subKey == null)
                    {
                        Console.WriteLine($"The registry key '{subKeyPath}' does not exist.");
                        return results;
                    }

                    // Enumerate all subkeys
                    foreach (var subKeyName in subKey.GetSubKeyNames())
                    {
                        var subKeyValues = new Dictionary<string, object>();

                        using (var currentSubKey = subKey.OpenSubKey(subKeyName))
                        {
                            if (currentSubKey != null)
                            {
                                // Enumerate all values in the subkey
                                foreach (var valueName in currentSubKey.GetValueNames())
                                {
                                    subKeyValues[valueName] = currentSubKey.GetValue(valueName);
                                }
                            }
                        }

                        results[subKeyName] = subKeyValues;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while enumerating the registry key: {ex.Message}");
            }

            return results;
        }

        public static List<string> EnumerateRegistryKeyAlt(RegistryKey hKey, string subKey = null)
        {
            var result = new List<string>();

            try
            {
                using (var targetKey = string.IsNullOrEmpty(subKey) ? hKey : hKey.OpenSubKey(subKey))
                {
                    if (targetKey == null)
                    {
                        Console.WriteLine($"The registry key '{subKey}' does not exist.");
                        return result;
                    }

                    // Enumerate subkeys
                    foreach (var subKeyName in targetKey.GetSubKeyNames())
                    {
                        result.Add(subKeyName);
                    }

                    // Enumerate values (optional, depending on your requirements)
                    foreach (var valueName in targetKey.GetValueNames())
                    {
                        result.Add(valueName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while enumerating the registry key: {ex.Message}");
            }

            return result;
        }

        // Helper method to rename registry key
        public static void RenameRegistryKey(RegistryKey registryKey, string oldName, string newName)
        {
            try
            {
                // We need to create the new key first, then copy values from the old one
                using (RegistryKey newKey = registryKey.CreateSubKey(newName))
                {
                    using (RegistryKey oldKey = registryKey.OpenSubKey(oldName))
                    {
                        if (oldKey != null)
                        {
                            foreach (var valueName in oldKey.GetValueNames())
                            {
                                newKey.SetValue(valueName, oldKey.GetValue(valueName));
                            }
                        }
                    }
                }

                // After copying, delete the old key
                registryKey.DeleteSubKey(oldName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error renaming registry key from {oldName} to {newName}: {ex.Message}");
            }
        }

        public static bool RegistryKeyExists(string keyPath)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }
        public static string ExtractRegistryKey(string fix)
        {
            // Use regular expression to extract the key from the string
            var match = System.Text.RegularExpressions.Regex.Match(fix, @"([^:]+):.*");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
        public static void ExportKey(string key)
        {
            string cleanedKey = Regex.Replace(key, @"(?i)ExportKey:\s*(.+)", "$1").Trim();
            cleanedKey = Regex.Replace(cleanedKey, @"^\s+|\s+$", "");
            cleanedKey = Regex.Replace(cleanedKey, @"(^\*+|\*+$)", "");
            cleanedKey = Regex.Replace(cleanedKey, @"(^\?+|\?+$)", "");
            cleanedKey = Regex.Replace(cleanedKey, @"(^;|;$)", "");
            cleanedKey = Regex.Replace(cleanedKey, @"(\*;\*|\*;|;\*)", ";");
            cleanedKey = Regex.Replace(cleanedKey, @"(\[|\])", "");

            // If Boot mode is Recovery, process accordingly
            if (SystemConstants.BootMode == "Recovery")
            {
                cleanedKey = RegistryRecoveryHandler.HandleRecovery(cleanedKey);
            }

            // Log the process
            //File.AppendAllText(Logger.WildlandsLogFile, "================== ExportKey: ===================" + Environment.NewLine);

            // Check for HKCU or other keys and replace with HKU if necessary
            if (cleanedKey.Contains("HKEY_CURRENT_USER") || cleanedKey.Contains("HKCU"))
            {
                string sid = RegistryUtils.GetUserSID(Environment.MachineName + "\\" + Environment.UserName);
                if (!string.IsNullOrEmpty(sid))
                {
                    cleanedKey = cleanedKey.Replace("HKCU", "HKU\\" + sid);
                }
            }

            // Append to the log the processed key
            //File.AppendAllText(Logger.WildlandsLogFile, "[" + cleanedKey + "]" + Environment.NewLine);

            // Call a method to process the registry export
            ProcessRegistryKeyExport(cleanedKey);
        }
        // Placeholder method for processing registry key export
        public static void ProcessRegistryKeyExport(string registryKey)
        {
            // Implement the logic to export the registry key (e.g., using reg export command)
            Console.WriteLine($"Processing registry export for: {registryKey}");
        }

        public static IntPtr OpenRegistryKey(string key, bool writable)
        {
            // Open the registry key with the appropriate access rights
            var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable ? RegistryRights.WriteKey : RegistryRights.ReadKey);

            // Check if the key was opened successfully
            if (registryKey != null)
            {
                // Use DangerousGetHandle to retrieve the raw handle as IntPtr
                return registryKey.Handle.DangerousGetHandle();
            }

            // Return IntPtr.Zero if the key couldn't be opened
            return IntPtr.Zero;
        }

        private static void CloseRegistryKey(IntPtr hKey)
        {
            RegCloseKey(hKey);
        }

        
        public static void CopyRegistryKey(string sourceKeyPath, string destKeyPath, bool deleteSource = false)
        {
            using (RegistryKey sourceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sourceKeyPath))
            {
                if (sourceKey == null)
                {
                    Console.WriteLine($"Source registry key not found: {sourceKeyPath}");
                    return;
                }

                using (RegistryKey destKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(destKeyPath))
                {
                    if (destKey == null)
                    {
                        Console.WriteLine($"Failed to create destination registry key: {destKeyPath}");
                        return;
                    }

                    foreach (string valueName in sourceKey.GetValueNames())
                    {
                        object valueData = sourceKey.GetValue(valueName);
                        RegistryValueKind valueKind = sourceKey.GetValueKind(valueName);
                        destKey.SetValue(valueName, valueData, valueKind);
                    }

                    foreach (string subKeyName in sourceKey.GetSubKeyNames())
                    {
                        CopyRegistryKey($"{sourceKeyPath}\\{subKeyName}", $"{destKeyPath}\\{subKeyName}");
                    }
                }
            }

            if (deleteSource)
            {
                DeleteRegistryKey(sourceKeyPath);
            }
        }


        // Example method to adjust or correct the registry key path
        public static string AdjustRegistryKey(string keyPath)
        {
            // You can add logic to modify or adjust the key path if needed
            // For example, correcting case sensitivity, handling missing parts, etc.

            if (string.IsNullOrEmpty(keyPath))
                throw new ArgumentException("Key path cannot be null or empty");

            // Example: Ensure that the key path is properly formatted
            if (!keyPath.StartsWith(@"HKEY"))
            {
                keyPath = @"HKEY_LOCAL_MACHINE\" + keyPath;  // Modify this as needed for your scenario
            }

            return keyPath;  // Return the adjusted key path
        }

        // Method to check registry key permissions
        public static void CheckRegistryKeyPermissions(string keyPath)
        {
            try
            {
                // Open the registry key with read-only access
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, false))
                {
                    if (key != null)
                    {
                        // Get the security information for the key
                        RegistrySecurity security = key.GetAccessControl();

                        Console.WriteLine("Permissions for key: " + keyPath);

                        // Log the permissions for each user or group
                        foreach (AuthorizationRule rule in security.GetAccessRules(true, true, typeof(NTAccount)))
                        {
                            if (rule is RegistryAccessRule registryRule)
                            {
                                Console.WriteLine($"User: {registryRule.IdentityReference}, Access: {registryRule.AccessControlType}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to open key: {keyPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking permissions for {keyPath}: {ex.Message}");
            }
        }


        public static bool CheckRegistryKeyLocked(string key)
        {
            string ret = RegistryUtils.GetSecDes(key, 4);  // Replace with your actual method for getting the security descriptor

            if (ret.Contains("(D;") || !System.Text.RegularExpressions.Regex.IsMatch(ret, @"A;.*?;KA;;;BA"))
            {
                return true;
            }

            return false;
        }


        public static bool CreateRegistryKey(string fullKey, uint access = 0xF003F)
        {
            try
            {
                // Extract root key (e.g., HKEY_LOCAL_MACHINE, HKEY_CURRENT_USER)
                string[] keyParts = fullKey.Split('\\');
                string rootKey = keyParts[0];
                string registryPath = fullKey.Substring(rootKey.Length + 1);

                // Open the appropriate root registry key based on rootKey
                RegistryKey baseKey;
                if (rootKey == "HKEY_LOCAL_MACHINE")
                {
                    baseKey = Microsoft.Win32.Registry.LocalMachine;
                }
                else if (rootKey == "HKEY_CURRENT_USER")
                {
                    baseKey = Microsoft.Win32.Registry.CurrentUser;
                }
                else if (rootKey == "HKEY_USERS")
                {
                    baseKey = Microsoft.Win32.Registry.Users;
                }
                else
                {
                    throw new ArgumentException("Unsupported registry root key");
                }

                // Create or open the subkey under the specified root key
                using (RegistryKey key = baseKey.CreateSubKey(registryPath))
                {
                    if (key != null)
                    {
                        Console.WriteLine($"Registry key {fullKey} created successfully.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create registry key {fullKey}.");
                        return false;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied while creating registry key: {ex.Message}");
                return false;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Invalid argument while creating registry key: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating registry key: {ex.Message}");
                return false;
            }
        }


        public static bool IsInvalidKey(string key)
        {
            // Check for invalid keys (this is just an example, use real logic)
            return key.Contains("Invalid");
        }
        // Method to delete a registry key (similar to DELKEY in AutoIt)


        public static void ScanSpecificKey(string keyPath)
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var valueData = key.GetValue(valueName)?.ToString();
                     //   Logger.AddToRegistryEntries($"Key: {keyPath}, ValueName: {valueName}, ValueData: {valueData}");
                    }
                }
            }
        }

        public static RegistryKey GetRegistryKey(string hive, string subKey)
        {
            RegistryKey baseKey;

            // Traditional switch statement
            switch (hive)
            {
                case "HKLM":
                    baseKey = Microsoft.Win32.Registry.LocalMachine;
                    break;
                case "HKCU":
                    baseKey = Microsoft.Win32.Registry.CurrentUser;
                    break;
                case "HKU":
                    baseKey = Microsoft.Win32.Registry.Users;
                    break;
                default:
                    throw new ArgumentException("Invalid hive specified.");
            }

            return baseKey?.OpenSubKey(subKey);
        }

        private static readonly List<string> LoadedKeys = new List<string>();

        public static string[] LoadKeys()
        {
            // For demonstration, returning mock data
            return LoadedKeys.ToArray();
        }

        public static bool DeleteRegistryKeyAlt(IntPtr registryHandle)
        {
            try
            {
                // Use the registry handle to delete the key
                // Example placeholder logic (adjust based on your requirements):
                Console.WriteLine($"Deleting registry key with handle: {registryHandle}");
                return true; // Indicate success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key: {ex.Message}");
                return false; // Indicate failure
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(IntPtr hKey);

        public static void CloseRegistryKey(ref IntPtr registryHandle)
        {
            if (registryHandle != IntPtr.Zero)
            {
                RegCloseKey(registryHandle); // Close the handle
                registryHandle = IntPtr.Zero; // Set to zero after closing
            }
        }

        public static string ConvertRegistryKey(IntPtr data, int length)
        {
            // Convert IntPtr to string, e.g., using Marshal
            string stringData = Marshal.PtrToStringAnsi(data); // Adjust as needed
                                                               // Process stringData
            return $"Converted: {stringData}";
        }
        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegEnumKeyEx(
            IntPtr hKey,
            uint index,
            System.Text.StringBuilder lpName,
            ref uint lpcName,
            IntPtr lpReserved,
            IntPtr lpClass,
            IntPtr lpcClass,
            IntPtr lpftLastWriteTime
        );


        public static uint GetRootKey(string key)
        {
            // Extract the root part of the key (before the first backslash)
            string rootKey = key.Split('\\')[0];
            if (string.IsNullOrEmpty(rootKey))
            {
                rootKey = key;
            }

            // Map the root key to its corresponding value
            switch (rootKey.ToUpperInvariant())
            {
                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    return 2147483650;
                case "HKEY_USERS":
                case "HKU":
                    return 2147483651;
                case "HKEY_CURRENT_USER":
                case "HKCU":
                    return 2147483649;
                case "HKEY_CLASSES_ROOT":
                case "HKCR":
                    return 2147483648;
                case "HKEY_CURRENT_CONFIG":
                case "HKCC":
                    return 2147483653;
                default:
                    throw new ArgumentException("Invalid root key specified.", nameof(key));
            }
        }
    }
}

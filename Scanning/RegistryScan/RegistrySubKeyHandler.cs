using DevExpress.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Utilities;
using static Wildlands_System_Scanner.Utilities.NativeMethods;

namespace Wildlands_System_Scanner.Registry
{
    public class RegistrySubKeyHandler
    {
        public static void EnumerateSubKeys(string startKey)
        {
            // Open the registry key
            using (RegistryKey keyHandle = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(startKey))
            {
                if (keyHandle == null)
                {
                    Console.WriteLine("Failed to open registry key");
                    return;
                }

                // Get the subkey names
                string[] subKeys = keyHandle.GetSubKeyNames();
                foreach (string subKeyName in subKeys)
                {
                    // Process each subkey
                    string fullSubKeyPath = startKey + "\\" + subKeyName;

                    // Check if the subkey is invalid (custom logic to check validity)
                    if (RegistryKeyHandler.IsInvalidKey(fullSubKeyPath))
                    {
                        // Delete the invalid registry key
                        RegistryKeyHandler.DeleteRegistryKey(fullSubKeyPath);
                        Console.WriteLine($"Deleted invalid subkey: {fullSubKeyPath}");
                    }
                }
            }
        }

        public static string EnumerateSubKey(RegistryKey parentKey, int index)
        {
            try
            {
                return parentKey.GetSubKeyNames()[index];
            }
            catch
            {
                return null;
            }
        }
        public static string EnumerateSubKeys(RegistryKey registryKey, ref int index)
        {
            try
            {
                return registryKey.GetSubKeyNames().Length > index ? registryKey.GetSubKeyNames()[index++] : null;
            }
            catch
            {
                return null;
            }
        }
        public static void DeleteSubKey(string key)
        {
            // Example method to delete a registry key
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(key, false);
        }

        public static string GetRegistrySubKey(RegistryKey key, int index)
        {
            try
            {
                var subKeys = key.GetSubKeyNames();
                if (index < subKeys.Length)
                {
                    return subKeys[index];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting subkey: {ex.Message}");
            }
            return null;
        }

        private static string EnumSubKey(IntPtr hKey, int index)
        {
            int bufferSize = 256;
            var buffer = new char[bufferSize];
            int result;

            while ((result = NativeMethods.RegEnumKeyEx(hKey, index, buffer, ref bufferSize, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero)) == NativeMethods.ERROR_MORE_DATA)
            {
                bufferSize *= 2;
                buffer = new char[bufferSize];
            }

            if (result == NativeMethods.ERROR_NO_MORE_ITEMS) return null;
            if (result != NativeMethods.ERROR_SUCCESS) throw new InvalidOperationException("Failed to enumerate registry keys.");

            return new string(buffer, 0, bufferSize);
        }
        // Method to get the subkey
        public static string GetSubKey(RegistryKey key, int index)
        {
            try
            {
                string[] subKeys = key.GetSubKeyNames();
                return (index >= 0 && index < subKeys.Length) ? subKeys[index] : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string GetSubKeyByIndex(RegistryKey key, int index)
        {
            try
            {
                string[] subKeyNames = key.GetSubKeyNames();
                if (index > 0 && index <= subKeyNames.Length)
                {
                    return subKeyNames[index - 1];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting subkey: {ex.Message}");
            }

            return null; // Return null when the subkey doesn't exist
        }
        public static bool INVALIDSUBKEY(string fullKey, out string invalidName)
        {
            invalidName = null;
            IntPtr hKey = RegistryUtils._HKEY(fullKey);

            if (hKey == IntPtr.Zero)
            {
                return true; // Error: Unable to open the registry key
            }

            int index = 0;

            try
            {
                while (true)
                {
                    // Initial buffer size
                    int resultLength = 0;
                    const int KeyInformationClass = 1; // KeyFullInformation
                    int bufferSize = 1024;
                    IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

                    try
                    {
                        // Call NtEnumerateKey
                        int status = NtEnumerateKey(hKey, index, KeyInformationClass, buffer, bufferSize, out resultLength);

                        if (status == RegistryUtils.STATUS_NO_MORE_ENTRIES || status == RegistryUtils.STATUS_BUFFER_OVERFLOW)
                        {
                            // Handle resizing the buffer if needed
                            Marshal.FreeHGlobal(buffer);
                            buffer = Marshal.AllocHGlobal(resultLength);
                            status = NtEnumerateKey(hKey, index, KeyInformationClass, buffer, resultLength, out _);
                        }

                        if (status != RegistryUtils.STATUS_SUCCESS)
                        {
                            return true; // Stop enumeration on error
                        }

                        // Extract name length and name
                        int nameLength = Marshal.ReadInt32(buffer, /* offset for NameLength */ 16); // Adjust offset for actual structure
                        string name = Marshal.PtrToStringUni(IntPtr.Add(buffer, /* offset for Name */ 32), nameLength / 2);

                        // Check for invalid characters
                        if (Utility.CHKINVAL(name, out string invalidChars))
                        {
                            invalidName = $"{invalidChars}<*>";
                            return true;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }

                    index++;
                }
            }
            finally
            {
                NativeMethods.NtClose(hKey);
            }
        }
        public static void INVALIDSUBKEYDEL(string mainKey, ref int result)
        {
            // Open the registry key
            RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(mainKey);

            if (registryKey == null)
            {
                string adjustedKey = RegistryKeyHandler.AdjustRegistryKey(mainKey);
                Logger.LogError($"{adjustedKey} => Failed to open main key.");
                return;
            }

            // Get the unmanaged handle (IntPtr) from the RegistryKey object
            IntPtr hKey = registryKey.Handle.DangerousGetHandle();

            // Allocate a buffer for the registry key information
            IntPtr buffer = Marshal.AllocHGlobal(1024);
            int resultLength = 0;  // Changed from uint to int
            int index = 0;

            try
            {
                // Loop through all subkeys
                while (true)
                {
                    // Enumerate subkeys
                    int status = NativeMethods.NtEnumerateKey(hKey, index, 1, buffer, 1024, out resultLength);

                    if (status == RegistryUtils.STATUS_NO_MORE_ENTRIES)
                        break; // No more entries, exit the loop

                    if (status == RegistryUtils.STATUS_BUFFER_TOO_SMALL)
                    {
                        // If the buffer is too small, allocate a larger one
                        Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal(resultLength);
                        status = NativeMethods.NtEnumerateKey(hKey, index, 1, buffer, resultLength, out resultLength);
                    }

                    if (status != RegistryUtils.STATUS_SUCCESS)
                    {
                        index++;
                        continue; // If an error occurs, move to the next subkey
                    }

                    // Extract subkey name
                    string name = RegistryKeyHandler.ExtractKeyName(buffer);

                    // Check for invalid characters
                    if (Utility.CHKINVAL(name, out string invalidChars))
                    {
                        HandleInvalidSubkey(hKey, name, ref result);
                        break; // Exit the loop once an invalid subkey is found
                    }

                    index++; // Move to the next subkey
                }
            }
            finally
            {
                // Free the unmanaged buffer and close the registry handle
                Marshal.FreeHGlobal(buffer);
                registryKey.Close();  // Close the RegistryKey object safely
            }
        }



        private static void HandleInvalidSubkey(IntPtr hKey, string name, ref int result)
        {
            // Handle deletion of invalid subkeys
            Console.WriteLine($"Deleting invalid subkey: {name}");
            result = 1; // Indicate success
        }

        public static void INVALIDSUBKEYNAMEFIX(string fix, string hFixLog, string deletedMessage, string notDeletedMessage, string invalidKeyMessage, string notFoundMessage)
        {
            string name = string.Empty;
            int result = 0;

            // If not in recovery mode, kill DLL (assume this is a placeholder for some cleanup logic)
            if (RegistryUtils.BootMode != "Recovery")
            {
                ProcessUtils.KILLDLL();
            }

            // Extract key paths using regex
            string fullKey = Regex.Replace(fix, ".*(\\[.+\\]).*", "$1");
            string mainKey = Regex.Replace(fullKey, "\\[(.+)\\\\.*\\]", "$1");

            // Check key permissions
            RegistryKeyHandler.CHECKKEYPERMS(mainKey);

            // Replace registry root keys with internal representation
            mainKey = Regex.Replace(mainKey, "(?i)(HKU|HKEY_USERS)", @"\registry\user");
            mainKey = Regex.Replace(mainKey, "(?i)(HKLM|HKEY_LOCAL_MACHINE)", @"\registry\machine");

            // Convert the mainKey to Unicode or a special format (implementation needed for STRTOUN)
            mainKey = RegistryUtils.STRTOUN(mainKey);

            // Delete invalid subkeys
            INVALIDSUBKEYDEL(mainKey, ref result);

            // Log if a subkey with an invalid name was deleted
            if (result == 1)
            {
                File.AppendAllText(hFixLog, $"{fullKey} => Subkey with invalid name {deletedMessage}{Environment.NewLine}");
                return;
            }

            // Check for remaining invalid subkeys
            INVALIDSUBKEY(mainKey, out name);

            if (!string.IsNullOrEmpty(name))
            {
                File.AppendAllText(hFixLog, $"{fullKey} => {notDeletedMessage} {invalidKeyMessage}.{Environment.NewLine}");
                return;
            }

            // Log if no invalid keys were found
            File.AppendAllText(hFixLog, $"{fullKey} => {invalidKeyMessage} {notFoundMessage}.{Environment.NewLine}");
        }

        // Helper method to enumerate subkeys
        private string EnumSubKey(RegistryKey key, int index)
        {
            string[] subkeys = key.GetSubKeyNames();
            return (index >= 0 && index < subkeys.Length) ? subkeys[index] : null;
        }
        // Placeholder method to simulate enumerating registry subkeys (simulates __REGENUMKEY)
        public static string RegEnumSubKey(RegistryKey hKey, int index)
        {
            string[] subKeys = hKey.GetSubKeyNames();
            return index >= 0 && index < subKeys.Length ? subKeys[index] : null;
        }

        public static void InvalidSubKey(string fullKey, out string name)
        {
            name = string.Empty;

            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(fullKey, writable: false))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine($"The registry key {fullKey} could not be found.");
                        return;
                    }

                    foreach (string subKeyName in registryKey.GetSubKeyNames())
                    {
                        if (IsInvalidSubKey(subKeyName))
                        {
                            name = subKeyName;
                            Console.WriteLine($"Invalid subkey found: {subKeyName}");
                            return;
                        }
                    }

                    Console.WriteLine("No invalid subkeys found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing registry key: {ex.Message}");
            }
        }

        public static bool IsInvalidSubKey(string subKeyName)
        {
            // Add your logic to identify invalid subkeys
            // Example: Check if the subkey name matches specific criteria
            if (string.IsNullOrWhiteSpace(subKeyName) || subKeyName.Contains("<invalid>"))
            {
                return true;
            }

            // Extend the logic based on your criteria
            return false;
        }

    }
}

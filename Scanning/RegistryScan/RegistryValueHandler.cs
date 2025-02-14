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
using static Wildlands_System_Scanner.Utilities.NativeMethods;

namespace Wildlands_System_Scanner.Registry
{
    public class RegistryValueHandler
    {

        private static string BOOTM = "";
        private static string _ERR0 = "Error";
        private static string VAL0 = "value";
        public const uint ERROR_ACCESS_DENIED = 5; // Access denied error code
        private static string RESTORED = "restored successfully";
        private static string ERRSV = "Error setting value.";

        private const uint KEY_QUERY_VALUE = 0x0001;
        private const uint STATUS_SUCCESS = 0x00000000;
        private const uint STATUS_NO_MORE_ENTRIES = 0x00000103;
        private const uint NTSTATUS_NO_MORE_ENTRIES = 0xC0000034;

        public static string TryReadRegistryValue(RegistryKey key, string subKey, string valueName)
        {
            using (RegistryKey subKeyKey = key.OpenSubKey(subKey))
            {
                if (subKeyKey != null)
                {
                    object value = subKeyKey.GetValue(valueName);
                    return value?.ToString();
                }
            }

            return null;
        }

        public static string ReadRegistryValueRegKey(RegistryKey baseKey, string subKeyPath)
        {
            using (var key = baseKey.OpenSubKey(subKeyPath))
            {
                if (key == null) return null;

                // Replace 'ValueName' with the appropriate value name you are looking for
                object value = key.GetValue("ValueName");
                return value?.ToString();
            }
        }

        public static string ReadRegistryValueRegKey(RegistryKey baseKey, string subKeyPath, string valueName)
        {
            using (var key = baseKey.OpenSubKey(subKeyPath))
            {
                if (key == null)
                    return null; // Subkey does not exist

                object value = key.GetValue(valueName);
                return value?.ToString(); // Return the registry value as a string
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

        public static void RestoreRegistryValue4Args(string keyPath, string name, string type, string value)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
            {
                if (key != null)
                {
                    key.SetValue(name, value, RegistryValueKind.String);
                    Console.WriteLine($"Restored: {name} => {value}");
                }
            }
        }

        private static Dictionary<string, string> GetRegistryValues(string registryPath)
        {
            Dictionary<string, string> registryValues = new Dictionary<string, string>();

            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key == null) return null;

                foreach (string valueName in key.GetValueNames())
                {
                    string valueData = key.GetValue(valueName)?.ToString();
                    if (!string.IsNullOrEmpty(valueData))
                    {
                        registryValues[valueName] = valueData;
                    }
                }
            }

            return registryValues;
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
        public static string ReadRegistryValue(string key, string valueName)
        {
            try
            {
                using (var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        var value = registryKey.GetValue(valueName);
                        return value != null ? value.ToString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry value: {ex.Message}");
            }

            return null;
        }

        public static string ReadRegistryValue(string key)
        {
            try
            {
                using (var subKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key))
                {
                    if (subKey != null)
                    {
                        return subKey.GetValue(null)?.ToString();
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
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

        public static string GetRegistryValue(string keyPath, string valueName)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    return key?.GetValue(valueName)?.ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool DELETEVAL(string startKey, string key)
        {
            IntPtr handle = RegistryUtils._HKEY(startKey, 1);
            if (handle == IntPtr.Zero)
            {
                // Handle error if the first handle is invalid
                return false;
            }

            uint index = 0;
            bool check = false;

            // Create structures to hold the registry value info
            IntPtr svi = IntPtr.Zero;
            uint bufferSize = 2048;

            while (true)
            {
                svi = Marshal.AllocHGlobal((int)bufferSize);
                uint resultLength = 0;
                int ret = NtEnumerateValueKey(handle, index, 0, svi, bufferSize, ref resultLength);

                // Check if we reached the end of the values
                if (ret == RegistryUtils.NTSTATUS_NO_MORE_ENTRIES)
                {
                    break;
                }

                // Handle buffer resizing if needed
                if (ret == RegistryUtils.NTSTATUS_BUFFER_TOO_SMALL)
                {
                    bufferSize += 2048;
                    continue;
                }

                if (ret != RegistryUtils.STATUS_SUCCESS)
                {
                    // Error in enumeration
                    return false;
                }

                // Extract name and validate
                string name = GetValueName(svi);
                bool invalid = CheckInvalid(name);

                if (invalid)
                {
                    // Handle invalid value
                    IntPtr secondHandle = RegistryUtils._HKEY(startKey, 2);
                    if (secondHandle == IntPtr.Zero)
                    {
                        // Handle error if the second handle is invalid
                        return false;
                    }


                    // Deleting the value
                    IntPtr valueNamePtr = Marshal.StringToHGlobalUni(name);
                    int deleteRet = NtDeleteValueKey(handle, valueNamePtr);
                    if (deleteRet != RegistryUtils.STATUS_SUCCESS)
                    {
                        // Error in Deleting Value
                        return false;
                    }

                    // Flush changes
                    NtClose(handle);
                    check = true;
                }

                index++;
            }

            NtClose(handle);
            return check;
        }

        public static bool CheckInvalid(string name)
        {
            // Example: Check if name is null or empty
            if (string.IsNullOrEmpty(name))
            {
                return true; // Invalid if empty or null
            }

            // Example: Check if name contains any invalid characters (e.g., digits or special symbols)
            if (name.Any(c => !char.IsLetterOrDigit(c)))
            {
                return true; // Invalid if it contains non-alphanumeric characters
            }

            return false; // Otherwise, valid
        }

        private static string GetValueName(IntPtr svi)
        {
            // Simulate getting the value name from the structure (this is a placeholder)
            return "SampleName";
        }

        // Main method to delete registry key values
        public bool DELETEVALUE(string fix, string key = "", string res = "")
        {
            // Modify the input string by removing spaces
            fix = fix.Replace(" | ", "|");

            // Extract registry key and value to delete
            string aKey = RegistryUtils.ExtractKey(fix);
            string value = ExtractValue(fix);

            if (string.IsNullOrEmpty(value)) value = string.Empty;

            // Handle Recovery Mode
            string keyAsIs = aKey;
            if (BOOTM == "Recovery")
            {
                aKey = RecoveryHandler.HandleRecovery(aKey);
                keyAsIs = RecoveryHandler.HandleRecovery(keyAsIs, false);
            }

            // Try to delete registry value
            if (DeleteRegistryValueInt(aKey, value) == 1)
            {
                File.AppendAllText(Logger.WildlandsLogFile, $"{keyAsIs} => {value} Deleted\n");
                return true;
            }

            // Continue deleting logic if needed
            string fullKey = RegistryKeyHandler.TransformRegistryKey(aKey);
            string startKey = RegistryUtils.ExtractStartKey(aKey);

            // Handle specific registry deletion logic
            // Initialize 'ret' for GetRegistryHandle, which returns an int
            int ret = RegistryUtils.GetRegistryHandle(fullKey, 131097, 1);

            // Create the registry key, which returns a bool, so we should use a bool variable for the result
            bool keyCreated = RegistryKeyHandler.CreateRegistryKey(fullKey);

            // Check if the registry handle is valid
            if (ret == 3221225506)
            {
                RegistryKeyHandler.UnlockRegistryKey(aKey); // Unlock the registry key

                // Now check the registry handle again
                if (RegistryUtils.GetRegistryHandle(fullKey) == 3221225524)
                {
                    // Set 'war' to 'res' if not null, otherwise set it to "Deleted"
                    string war = res ?? "Deleted";

                    // Ensure the registry key is not invalid before appending to the log
                    if (!RegistryKeyHandler.IsInvalidKey(startKey, keyAsIs))
                    {
                        // Append to the log file
                        File.AppendAllText(Logger.WildlandsLogFile, $"{keyAsIs} => {war}\n");
                    }
                }

                // Attempt to create the registry key again
                keyCreated = RegistryKeyHandler.CreateRegistryKey(fullKey); // Using the bool result
            }


            // Check for error codes
            ErrorHandler.HandleErrorCodes((uint)ret, keyAsIs);

            return true;
        }

        // Extract the registry value to delete
        private string ExtractValue(string fix)
        {
            return System.Text.RegularExpressions.Regex.Replace(fix, @"(?i).+?\|(.*)", "$1").Trim();
        }

        private string ReadRegistryValue(RegistryKey key, string valueName)
        {
            try
            {
                return key.GetValue(valueName, string.Empty)?.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public void DELETEVALUE(ref string fix, string bootMode)
        {
            // Replace spaces around pipes and clean the string
            fix = fix.Replace(" | ", "|");

            // Extract the registry key
            string aKey = Regex.Replace(fix, @"(?i)delete\s*value:\s*(.+)\|.*", "$1");
            aKey = Regex.Replace(aKey, @"(^\[|^""|^;|;$|""$|\]$)", "");

            // Extract the value
            string value = Regex.Replace(fix, @"(?i).+?\|(.*)", "$1");

            if (value == "\"\"\"\"")
                value = "";

            // If in "Recovery" mode, modify the registry key
            if (bootMode == "Recovery")
                aKey = RegistryRecoveryHandler.RmTor(aKey);

            // Call the DELVALUE method to delete the key-value pair
            DELVALUE(aKey, value);
        }

        // Method to delete a registry value
        public static void DELVALUE(string AKEY, string VALUE)
        {
            if (VALUE.Contains("<*>"))
            {
                DELVALUEINVALID(AKEY, VALUE);
                return;
            }

            string KEY = AKEY;

            // If in "Recovery" mode, modify the key
            if (BOOTM == "Recovery")
            {
                AKEY = RegistryRecoveryHandler.RmTon(AKEY);
            }

            // Attempt to delete the registry value
            if (RegistryUtils.RegDelete(KEY, VALUE) == 1)
            {
                Logger.Deleted(AKEY + "\\" + VALUE);
                return;
            }

            var RET = RegOpenKeyEx3(KEY, 0, false);

            if (RET != IntPtr.Zero)
            {
                if (RegistryUtils.RegDelete(KEY, VALUE) == 1)
                {
                    Logger.Deleted(AKEY + "\\" + VALUE);
                    return;
                }

                RET = RegOpenKeyEx3(KEY, 0, false); // Retry

                if (RET != IntPtr.Zero)
                {
                    Logger.NDELETED(AKEY + "\\" + VALUE);
                    return;
                }
            }

            if (RET != IntPtr.Zero)
            {
                RegistryUtils.SETREGACE(KEY);

                if (RegistryUtils.RegDelete(KEY, VALUE) == 1)
                {
                    Logger.Deleted(AKEY + "\\" + VALUE);
                    return;
                }
            }

            if (RET == IntPtr.Zero)
            {
                Logger.NFOUND(AKEY);
                return;
            }

            File.AppendAllText(Logger.WildlandsLogFile, $"{AKEY} => {_ERR0} = {RET}\n");
        }

        public static void DELVALUEINVALID(string KEY, string VALUE)
        {
            string AKEY = KEY;

            // If boot mode is Recovery, adjust the key using _RMTON method
            if (BOOTM == "Recovery")
            {
                AKEY = RegistryRecoveryHandler.RmTon(AKEY); // Assuming this method exists
            }

            string FULLKEY = RegistryUtils.HKEYTRANS(KEY); // Assuming this method exists

            // Perform the actual deletion of the registry value
            bool RET = DELETEVAL(FULLKEY, KEY); // Assuming DELETEVAL is another method

            if (RET)
            {
                // If successfully deleted, log the deletion
                Logger.Deleted(AKEY + "\\" + VALUE); // Assuming DELETED is a method
            }
            else
            {
                // Log the deletion action to the log file
                Logger.NDELETED(AKEY + "\\" + VALUE); // Log the action first
                File.AppendAllText(Logger.WildlandsLogFile,
                    $"{AKEY}\\{VALUE} => {VAL0} Deleted.\n"); // Then append the appropriate message
                // Call the method without expecting a return value
                Logger.NFOUND(AKEY + "\\" + VALUE); // Log the "Not Found" information
            }

        }

        // Method to restore a registry value
        public static void RESTOREVAL(string key, string valueName, RegistryValueKind valueType, string valueData)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
                {
                    if (registryKey != null)
                    {
                        registryKey.SetValue(valueName, valueData, valueType);
                        Console.WriteLine(
                            $"Successfully restored the value: {valueName} with data: {valueData} to {key}");
                    }
                    else
                    {
                        Console.WriteLine($"Registry key '{key}' not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring registry value: {ex.Message}");
            }
        }

        // Helper method to read registry value
        private string ReadRegistryValueClassesRoot(string key)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        return registryKey.GetValue("")?.ToString() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry key {key}: {ex.Message}");
            }

            return string.Empty;
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

        public static bool WriteRegistryValue(string keyPath, string valueName, object data,
            RegistryValueKind valueKind)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Failed to open or create registry key: {keyPath}");
                        return false;
                    }

                    key.SetValue(valueName, data, valueKind);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing registry value: {ex.Message}");
                return false;
            }
        }

        public static bool DeleteRegistryValueLocalMachine(string keyPath, string valueName)
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {keyPath}");
                        return false;
                    }

                    key.DeleteValue(valueName, throwOnMissingValue: false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry value: {ex.Message}");
                return false;
            }
        }

        public static bool ValueExists(string keyPath, string valueName)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
            {
                return key?.GetValue(valueName) != null;
            }
        }

        public static long ListRegistryValues(IntPtr hKey)
        {
            TAGKEY_VALUE_PARTIAL_INFORMATION svi = new TAGKEY_VALUE_PARTIAL_INFORMATION();
            uint index = 0;
            uint resultLength = 0;
            long returnCode = 0; // This will be used to return status as a long value

            List<string[]> entries = new List<string[]>();

            while (true)
            {
                int ret = NtEnumerateValueKey(hKey, index, 2, Marshal.UnsafeAddrOfPinnedArrayElement(new byte[Marshal.SizeOf(typeof(TAGKEY_VALUE_PARTIAL_INFORMATION))], 0), (uint)Marshal.SizeOf(typeof(TAGKEY_VALUE_PARTIAL_INFORMATION)), ref resultLength);

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

        public static void ProcessRegistryValues(IntPtr hKey)
        {
            uint index = 0;
            while (true)
            {
                // Enumerate and process registry values here
                // Simulate calling EnumProcessModules or other actions
                // Use DllCalls and proper handling of registry keys
                index++;
                if (index > 100) break; // Just a placeholder condition
            }
        }

        private static string ReadRegistryValueLocalMachine(string key, string value)
        {
            using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
            {
                if (registryKey != null)
                {
                    return registryKey.GetValue(value)?.ToString();
                }
            }

            return string.Empty;
        }

        public static string EnumRegistryValue(string key, int index)
        {
            using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
            {
                if (registryKey != null)
                {
                    string[] values = registryKey.GetValueNames();
                    if (index < values.Length)
                    {
                        return values[index];
                    }
                }
            }

            return string.Empty;
        }

        private static string GetRegistryValueLocalMachine(string subKey, string valueName)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    return value as string;
                }
            }

            return null; // Return null if the key or value doesn't exist.
        }

        private static void RestoreRegistryValueLocalMachine(string subKey, string valueName, string valueData)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKey, writable: true))
            {
                if (key != null)
                {
                    key.SetValue(valueName, valueData, RegistryValueKind.String);
                    Console.WriteLine($"Registry value restored: {subKey}\\{valueName} = {valueData}");
                }
            }
        }

        private static void DELVALUELocalMachine(string key, string valueName)
        {
            using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (registryKey != null)
                {
                    registryKey.DeleteValue(valueName, throwOnMissingValue: false);
                    Console.WriteLine($"Deleted {valueName} from {key}");
                }
                else
                {
                    Console.WriteLine($"Key {key} not found.");
                }
            }
        }

        // Get registry value with error checking
        private static string GetRegistryValue(string keyPath)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    return key.GetValue("")?.ToString();
                }
            }

            return string.Empty;
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

        public static void RESTOREVAL(string key, string val, string vType, string vData)
        {
            string aKey = RegistryRecoveryHandler.RmTon(key);
            bool result = RegistryUtils.RegWrite(key, val, vType, vData);

            if (string.IsNullOrEmpty(val))
            {
                val = "Default";
            }

            if (result)
            {
                File.AppendAllText(Logger.WildlandsLogFile,
                    $"{aKey}\\\"{val}\"=\"{vData.Replace("\v", " ")}\" => {VAL0} {RESTORED}{Environment.NewLine}");
            }

            RegistryUtils.CheckKeyLocked(key);

            result = RegistryUtils.RegWrite(key, val, vType, vData);

            if (result)
            {
                File.AppendAllText(Logger.WildlandsLogFile,
                    $"{aKey}\\\"{val}\"=\"{vData.Replace("\v", " ")}\" => {VAL0} {RESTORED}{Environment.NewLine}");
            }
            else
            {
                File.AppendAllText(Logger.WildlandsLogFile, $"{aKey}\\{val} => {ERRSV}{Environment.NewLine}");
            }
        }

        // Placeholder method to simulate _LISTVAL in AutoIt, which lists the values for a registry key
        public static List<List<string>> ListVal(RegistryKey hKey)
        {
            List<List<string>> list = new List<List<string>>();
            foreach (string valueName in hKey.GetValueNames())
            {
                string valueData = hKey.GetValue(valueName)?.ToString();
                list.Add(new List<string> { valueName, valueData });
            }

            return list;
        }

        // Method to delete a registry value (mimicking DELVALUE in AutoIt)
        private void DELVALUEUsers(string key, string valueName)
        {
            using (RegistryKey regKey = Microsoft.Win32.Registry.Users.OpenSubKey(key, writable: true))
            {
                if (regKey != null)
                {
                    regKey.DeleteValue(valueName, throwOnMissingValue: false);
                }
            }
        }

        // Method to write a value to the registry
        public void SetRegistryValue(string key, string valueName, string valueType, string valueData)
        {
            using (RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (regKey != null)
                {
                    if (valueType == "REG_SZ")
                    {
                        regKey.SetValue(valueName, valueData, RegistryValueKind.String);
                    }
                }
            }
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

        public static string RegEnumVal(string key, int index)
        {
            // Mocking the registry reading logic for demonstration
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

        // Helper method to enumerate values of a registry key
        private string EnumValue(RegistryKey key, string subkey, int index)
        {
            RegistryKey subKey = key.OpenSubKey(subkey);
            if (subKey == null) return null;

            string[] values = subKey.GetValueNames();
            return (index >= 0 && index < values.Length) ? values[index] : null;
        }

        // Helper method to read a registry value
        private string ReadRegistryValue(RegistryKey key, string subkey, string valueName)
        {
            RegistryKey subKey = key.OpenSubKey(subkey);
            if (subKey == null) return null;

            object value = subKey.GetValue(valueName);
            return value?.ToString();
        }

        // Placeholder method to restore registry values (simulates RESTOREVAL)
        private void RESTOREVALLocalMachine(string key, string valName, string valType, string valData)
        {
            using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (registryKey != null)
                {
                    registryKey.SetValue(valName, valData,
                        (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), valType));
                }
            }
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

        public static string GetRegistryValue(string registryKey, string subKey, string valueName)
        {
            // Open the base registry key
            using (RegistryKey baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey))
            {
                if (baseKey == null)
                {
                    return null; // Registry key not found
                }

                // Open the subkey
                using (RegistryKey targetKey = baseKey.OpenSubKey(subKey))
                {
                    if (targetKey == null)
                    {
                        return null; // Subkey not found
                    }

                    // Get the value and convert it to string
                    return targetKey.GetValue(valueName)?.ToString();
                }
            }
        }


        public static void RestoreRegistryValue(string key, string valueName, string valueType, string valueData)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, true))
                {
                    if (registryKey != null)
                    {
                        registryKey.SetValue(valueName, valueData, RegistryValueKind.String);
                        Console.WriteLine($"Restored registry value: {valueName} = {valueData}");
                    }
                    else
                    {
                        Console.WriteLine($"Registry key not found: {key}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring registry value: {ex.Message}");
            }
        }

        public static List<string> GetRegistryEnumValue(string registryKey)
        {
            List<string> values = new List<string>();

            try
            {
                // Open the registry key
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        // Enumerate through the subkeys and values
                        foreach (string valueName in key.GetValueNames())
                        {
                            values.Add(valueName);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Registry key does not exist.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing the registry: {ex.Message}");
            }

            return values;
        }

        public static string GetRegistryEnumValue(string registryKeyPath, int index)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath))
                {
                    if (key != null)
                    {
                        var valueNames = key.GetValueNames();
                        if (index >= 0 && index < valueNames.Length)
                        {
                            return valueNames[index];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exception
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null; // Return null if the value is not found or an error occurs
        }

        public static object GetValue(string registryKeyPath, string valueName, object defaultValue)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryKeyPath))
                {
                    if (key != null)
                    {
                        return key.GetValue(valueName, defaultValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing registry: {ex.Message}");
            }

            return defaultValue;
        }

        public static string GetRegistryValue(RegistryKey registryKey, int index)
        {
            // Access the value by index
            string[] valueNames = registryKey.GetValueNames();
            if (index >= 0 && index < valueNames.Length)
            {
                return registryKey.GetValue(valueNames[index])?.ToString();
            }

            return null;
        }

        // Modify DeleteRegistryValue to return a status code (int)
        public static int DeleteRegistryValueInt(string key, string value)
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

            // Assuming the deletion was successful
            Console.WriteLine($"Deleted registry value: {key}\\{value}");
            return 1; // Return 1 to indicate success
        }

        // Example root registry key handle (HKEY_LOCAL_MACHINE)
        public static IntPtr HKEY_LOCAL_MACHINE = new IntPtr(0x80000002);

        // Simulate _ROOTHK
        public static IntPtr GetRootKey(string key)
        {
            if (key.StartsWith("HKEY_LOCAL_MACHINE"))
            {
                return new IntPtr(0x80000002); // Example for HKEY_LOCAL_MACHINE
            }
            // Add other root key checks as needed
            return IntPtr.Zero;
        }

        // Simulate the _REGOPENKEYEX3 function in C#
        public static IntPtr RegOpenKeyEx3(string key, uint access = 131097, bool takeOwnership = false)
        {
            IntPtr hKey = GetRootKey(key);
            if (hKey == IntPtr.Zero) return IntPtr.Zero;

            string subKey = key.Substring(key.IndexOf('\\') + 1); // Extract subkey after the root

            IntPtr hSubKey;
            int result = NativeMethods.RegOpenKeyExAlt(hKey, subKey, 0, (uint)access, out hSubKey);

            if (result == ERROR_ACCESS_DENIED && takeOwnership)
            {
                // Handle access denied and privilege escalation here
                // For simplicity, assume we try to set the ownership and retry
                Console.WriteLine("Access Denied, attempting privilege escalation...");
                // Placeholder for setting privilege and retrying RegOpenKeyEx
                result = NativeMethods.RegOpenKeyExAlt(hKey, subKey, 0, (uint)524288, out hSubKey); // 524288 is a placeholder value
                if (result == 0)
                {
                    // If successful, we can return the handle to the subkey
                    return hSubKey;
                }
            }

            if (result != 0)
            {
                // Handle error if result is not 0 (success)
                Console.WriteLine($"Error opening key: {result}");
                return IntPtr.Zero;
            }

            return hSubKey; // Return the valid handle
        }

        // Method to restore or create a registry value
        public static void RestoreVal(string keyPath, string valueName, string valueType, string valueData)
        {
            try
            {
                // Open the registry key in write mode
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Error: Unable to open the registry key at {keyPath}");
                        return;
                    }

                    // Depending on the valueType (REG_SZ, etc.), set the appropriate value type
                    switch (valueType.ToUpper())
                    {
                        case "REG_SZ":
                            key.SetValue(valueName, valueData, RegistryValueKind.String);
                            break;
                        case "REG_DWORD":
                            key.SetValue(valueName, int.Parse(valueData), RegistryValueKind.DWord);
                            break;
                        // Add more cases for other registry value types if needed
                        default:
                            Console.WriteLine("Error: Unsupported registry value type.");
                            break;
                    }

                    Console.WriteLine($"Registry value '{valueName}' restored successfully at {keyPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring registry value: {ex.Message}");
            }
        }

        // Method to delete a registry value
        public static void DeleteValue(string keyPath, string valueName)
        {
            try
            {
                // Open the registry key in write mode
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Error: Unable to open the registry key at {keyPath}");
                        return;
                    }

                    // Check if the value exists
                    if (key.GetValue(valueName) != null)
                    {
                        key.DeleteValue(valueName);  // Delete the specified value
                        Console.WriteLine($"Registry value '{valueName}' deleted successfully from {keyPath}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: Registry value '{valueName}' not found in {keyPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry value: {ex.Message}");
            }
        }

        public static List<string[]> ListRegistryValuesList(IntPtr hKey)
        {
            var entries = new List<string[]>();

            // Your logic to enumerate registry values and populate entries.
            // For example:
            entries.Add(new string[] { "ValueName1", "Data1" });
            entries.Add(new string[] { "ValueName2", "Data2" });

            return entries;
        }

        public static void SetRegistryValue(string keyPath, string valueName, string valueData, RegistryValueKind valueKind)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
            {
                if (key == null)
                {
                    throw new InvalidOperationException($"Registry key {keyPath} not found.");
                }

                key.SetValue(valueName, valueData, valueKind);
            }
        }

    }
}


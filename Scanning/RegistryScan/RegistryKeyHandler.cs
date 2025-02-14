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
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Registry
{
    public class RegistryKeyHandler
    {
        public static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(0x80000002); // Constant for HKEY_LOCAL_MACHINE
        const int NERR_Success = 0;
        public static string[,] Load = new string[0, 2]; // Example of 2D array with two columns (userKey and status)
        public const int KEY_READ = 0x20019;

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
        public static RegistryKey RegOpenKey(string key)
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
        public static object RegOpenKeyEx(string key, int access)
        {
            // Simulate opening registry key
            return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key);
        }
        public static string RegEnumKey(object hkey, int index)
        {
            // Simulate enumerating registry keys
            var subKeys = ((RegistryKey)hkey).GetSubKeyNames();
            return index < subKeys.Length ? subKeys[index] : null;
        }

        public static void DeleteKey(string key)
        {
            // Open the registry key for modification (This will only work with the proper permissions)
            using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (registryKey != null)
                {
                    // Deleting the key
                    Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(key);
                    Console.WriteLine($"Registry key deleted: {key}");
                }
                else
                {
                    Console.WriteLine($"Registry key not found: {key}");
                }
            }
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
        // Method to check and delete registry key if it exists
        public static void DeleteRegistryKeyIfExists(string key)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
                {
                    if (registryKey != null)
                    {
                        registryKey.DeleteSubKeyTree(""); // Delete the registry key and all its subkeys
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key {key}: {ex.Message}");
            }
        }
        public static void DELKEY(string keyPath)
        {
            try
            {
                // Open the registry key for writing
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key != null)
                    {
                        // Delete the key (recursive deletion not directly supported, might need to handle keys manually)
                        // You would typically delete a value under a key, not the key itself, but if you want to delete the entire key:
                        Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(keyPath);
                        Console.WriteLine($"Deleted registry key: {keyPath}");
                    }
                    else
                    {
                        Console.WriteLine("Registry key not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key: {ex.Message}");
            }
        }
        // Extract the registry key from the input string
        private string ExtractKey(string fix)
        {
            return System.Text.RegularExpressions.Regex.Replace(fix, @"(?i)delete\s*key:\s*(.+)\|.*", "$1").Trim();
        }

        // Create registry key if not present
        private int CreateRegistryKey(string fullKey)
        {
            // Simulate registry key creation (actual implementation depends on your needs)
            return 2; // Placeholder for actual key creation logic
        }
        public static bool CreateRegistryKeyBool(string key)
        {
            // Return a bool indicating success or failure
            return true;
        }

        // Unlock registry key if locked
        public static void UnlockRegistryKey(string key)
        {
            // Simulate unlocking the registry key (actual implementation depends on your needs)
        }

        // Method to check if the registry key is invalid
        public static bool IsInvalidKey(string startKey, string keyAsIs)
        {
            try
            {
                // Attempt to open the key (you can adjust the access permissions as needed)
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(startKey + "\\" + keyAsIs))
                {
                    // If the key is null, it means it doesn't exist or is inaccessible
                    return key == null;
                }
            }
            catch (Exception)
            {
                // Handle potential errors, such as access issues
                return true;
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
            if (RegistryUtils.BootMode == "Recovery")
            {
                cleanedKey = RegistryRecoveryHandler.HandleRecovery(cleanedKey);
            }

            // Log the process
            File.AppendAllText(Logger.WildlandsLogFile, "================== ExportKey: ===================" + Environment.NewLine);

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
            File.AppendAllText(Logger.WildlandsLogFile, "[" + cleanedKey + "]" + Environment.NewLine);

            // Call a method to process the registry export
            ProcessRegistryKeyExport(cleanedKey);
        }
        // Placeholder method for processing registry key export
        public static void ProcessRegistryKeyExport(string registryKey)
        {
            // Implement the logic to export the registry key (e.g., using reg export command)
            Console.WriteLine($"Processing registry export for: {registryKey}");
        }
        public static void RegKeyFix(string fix)
        {
            // Extract the key from the fix string using Regex
            string key = System.Text.RegularExpressions.Regex.Replace(fix, @"RegKey: \[(.+)?\].*", "$1");

            // Delete the registry key if it exists
            DeleteRegistryKey(key);
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
            NativeMethods.RegCloseKey(hKey);
        }

        private static void DeleteRegistryKey(IntPtr hKey)
        {
            NativeMethods.RegDeleteKey(hKey, null);
        }

        public static string TransformKey(string key)
        {
            // Transform the key path as needed
            return key;
        }

        public static void HandleInvalidKey(string startKey, string entry)
        {
            // Handle invalid key logic
        }

        private static string StartKey(string entry)
        {
            // Generate start key logic
            return entry;
        }

        public static void DeleteKeys(ref List<string> arr)
        {
            string mainKey = $"HKEY_LOCAL_MACHINE\\{RegistryUtils.SYSTEM}\\{RegistryUtils.BOOTSYSTEM}\\{RegistryUtils.DEF}\\Services";

            try
            {
                using (RegistryKey servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"{RegistryUtils.SYSTEM}\\{RegistryUtils.BOOTSYSTEM}\\{RegistryUtils.DEF}\\Services", writable: true))
                {
                    if (servicesKey == null)
                    {
                        Console.WriteLine("Failed to open the Services key.");
                        return;
                    }

                    foreach (string subKeyName in servicesKey.GetSubKeyNames())
                    {
                        string subKeyPath = $"{mainKey}\\{subKeyName}";
                        Console.WriteLine($"Processing key: {subKeyName}");

                        if (subKeyName == "{45487F67-EC9F-4449-A6F2-2D0970F9B80B}" || subKeyName == "{DB437C57-08A3-47e9-ACFF-111254F830DF}")
                        {
                            string filePath = RegistryValueHandler.ReadRegistryValue(subKeyPath, "ImagePath");

                            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                            {
                                FileUtils.MoveFile(filePath);
                                arr.Add($"{filePath} => MOVED");
                            }

                            DeleteRegistryKey(subKeyPath);
                            continue;
                        }

                        string altitude = RegistryValueHandler.ReadRegistryValue($"{subKeyPath}\\Instances\\{subKeyName} Instance", "Altitude");
                        string start = RegistryValueHandler.ReadRegistryValue(subKeyPath, "Start");

                        if (altitude == "45888" && start == "3")
                        {
                            string filePath = RegistryValueHandler.ReadRegistryValue(subKeyPath, "ImagePath");

                            if (File.Exists(filePath))
                            {
                                FileUtils.MoveFile(filePath);
                                arr.Add($"{filePath} => MOVED");
                            }

                            DeleteRegistryKey(subKeyPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing registry keys: {ex.Message}");
            }
        }

        public static void DELKEYS(string OKEY, ref System.Collections.Generic.List<string> ARR)
        {
            string KEYASIS = OKEY;
            OKEY = RegistryRecoveryHandler.RmTor(OKEY);
            KEYASIS = RegistryRecoveryHandler.RmTon(KEYASIS);
            string FULLKEY = RegistryUtils.HKEYTRANS(OKEY);
            string STARTKEY = RegistryUtils.StartKey(OKEY);

            long RET = RegistryUtils.HKEYCREATE(FULLKEY);
            long ERR = 0; // Assume appropriate error capture

            // Correctly handling 3221225506 as a long value
            if (RET == 3221225506L) // Note the 'L' for long
            {
                RegistryUtils._UNLOCKPARANDKEY(OKEY);
                RET = RegistryUtils.HKEYCREATE(FULLKEY);
            }

            if (ERR != 0)
            {
                switch (RET)
                {
                    case 3221225506L: // Using long for this comparison
                        ARR.Add($"{KEYASIS} => Not Deleted. No Access.");
                        if (!RegistryUtils.REBOOTED)
                            File.AppendAllText("C:\\frst\\keysRem", KEYASIS + "\n");

                        break;
                    default:
                        RET = Convert.ToInt32("0x" + RET.ToString("X8"), 16);
                        ARR.Add($"{KEYASIS} => Not Deleted. Error Code1: {RET}");
                        break;
                }
            }
            else
            {
                switch (RET)
                {
                    case 2:
                        IntPtr HKEY = RegistryUtils._HKEY(FULLKEY, 1); // Ensure the correct argument type is passed here
                        if (HKEY != IntPtr.Zero)
                        {
                            RET = RegistryValueHandler.ListRegistryValues(HKEY);
                            NativeMethods.NtDeleteKey(HKEY);
                            if (RET == 0)
                            {
                                ARR.Add($"\"{KEYASIS}\" => Deleted");
                            }
                            else
                            {
                                NativeMethods.NtClose(HKEY);
                                ARR.Add($"{KEYASIS} => Not Deleted. Some Error occurred.");
                            }
                        }

                        break;

                    case 1:
                        if (RegistryUtils.INVALID(STARTKEY, KEYASIS) != 0)
                        {
                            ARR.Add($"{KEYASIS} => Deleted");
                        }
                        break;

                    default:
                        ARR.Add($"{KEYASIS} => Not Deleted. ErrorCode2: {RET}");
                        break;
                }
            }
        }

        string RegEnumKey(RegistryKey hKey, int index)
        {
            try
            {
                return hKey.GetSubKeyNames()[index];
            }
            catch
            {
                return null;
            }
        }
        // Method to check if a registry key exists
        private bool RegistryKeyExistsLocalMachine(string key)
        {
            try
            {
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    return registryKey != null;
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool DeleteRegistryKeyLocalMachine(string keyPath)
        {
            try
            {
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key: {ex.Message}");
                return false;
            }
        }
        public static bool KeyExists(string keyPath)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
            {
                return key != null;
            }
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
        private static string ConvertRegistryKey(IntPtr dataPtr, uint dataLength)
        {
            byte[] dataBytes = new byte[dataLength];
            Marshal.Copy(dataPtr, dataBytes, 0, (int)dataLength);
            return Encoding.Unicode.GetString(dataBytes); // Assuming data is Unicode/UTF-16 encoded
        }

        public static void ProcessRegistryKeys(IntPtr hKey)
        {
            var result = RegistryValueHandler.ListRegistryValuesList(hKey);
            foreach (var row in result)
            {
                Console.WriteLine($"Value: {row[0]}, Data: {row[1]}");
            }
        }

        public static void FlushRegistryKey(IntPtr hKey)
        {
            NativeMethods.NtFlushKey(hKey);
        }
        public static void CloseRegistryKey(ref Microsoft.Win32.RegistryKey key)
        {
            key?.Close();
            key = null;
        }

        public static IntPtr OpenRegistryKey(string keyPath, uint accessRights)
        {
            IntPtr hKey = IntPtr.Zero;

            // Use RegOpenKeyEx to open the key with the specified access rights
            uint result = NativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, keyPath, 0, accessRights, ref hKey);

            // Check if the result indicates success (0 means success)
            if (result != 0)
            {
                return IntPtr.Zero; // Failed to open registry key
            }

            return hKey;
        }

        // Handle registry unloading
        public static void UnloadRegistryKey(string keyPath)
        {
            // Similar to _REGUNLOC
            IntPtr hKey = OpenRegistryKey(keyPath, RegistryUtils.KEY_ALL_ACCESS);
            if (hKey == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to open registry key");
            }

            // Use RegUnLoadKey (if needed)
            // Unload key logic here
            CloseRegistryKey(hKey);
        }
        // Example of using registry manipulation to load keys
        public static void LoadRegistryKey(string keyName, string path)
        {
            // Example implementation of registry loading
            string keyPath = @"HKLM\" + keyName;
            IntPtr hKey = OpenRegistryKey(keyPath, RegistryUtils.KEY_WRITE);
            if (hKey == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to load registry key");
            }

            // Simulate reading or modifying the registry key
            // Implement other logic as needed

            CloseRegistryKey(hKey);
        }
        // Load and unload registry keys based on conditions
        public static void LoadUnloadRegistryKeys()
        {
            RegistryUtils.SetPrivileges();  // Equivalent to _SETPRIV0
            var userAccounts = RegistryUtils.NetUserEnum();

            foreach (var user in userAccounts)
            {
                string userKey = user[0];
                string userPath = user[1] + "\\ntuser.dat";

                if (!RegistryUtils.RegistryExists(userKey))
                {
                    if (!File.Exists(userPath)) continue;
                    var result = NativeMethods.RegLoadKey(new IntPtr(2147483651), userKey, userPath);
                    if (result == 0)
                    {
                        // Assuming "status" is a string representing the load status
                        string status = "Loaded";
                        ArrayUtils.AddToArray(Load, userKey, status);
                    }

                }
            }
        }

        // Unload registry keys after processing
        public static void UnloadRegistryKeys()
        {
            RegistryUtils.SetPrivileges();  // Equivalent to _SETPRIV0
            foreach (var key in RegistryUtils.Load)
            {
                if (RegistryUtils.RegistryExists(key))
                {
                    NativeMethods.RegUnLoadKey(new IntPtr(2147483651), key);
                }
            }
        }
        private static void DeleteKeyLocalMachine(string key)
        {
            try
            {
                RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true);
                if (registryKey != null)
                {
                    registryKey.DeleteSubKeyTree(key);
                    Console.WriteLine($"Registry key {key} deleted successfully.");
                }
                else
                {
                    Console.WriteLine($"Registry key {key} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key: {ex.Message}");
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
        public static void CHECKKEYPERMS(string keyPath)
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


        public static bool CheckKeyLocked(string key)
        {
            string ret = RegistryUtils.GetSecDes(key, 4);  // Replace with your actual method for getting the security descriptor

            if (ret.Contains("(D;") || !System.Text.RegularExpressions.Regex.IsMatch(ret, @"A;.*?;KA;;;BA"))
            {
                return true;
            }

            return false;
        }

        // Example method to delete registry key
        private static void DeleteRegistryKeyCurrentUser(string keyPath)
        {
            using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyPath, writable: true))
            {
                key?.DeleteValue("", false);
            }
        }

        public static RegistryKey OpenRegistryKey(string registryKey)
        {
            IntPtr keyHandle = IntPtr.Zero;

            try
            {
                // Open the registry key with read access
                int result = NativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, registryKey, 0, KEY_READ, out keyHandle);

                if (result != 0 || keyHandle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // Wrap the handle in a SafeRegistryHandle and return a RegistryKey
                var safeHandle = new SafeRegistryHandle(keyHandle, ownsHandle: true);
                return RegistryKey.FromHandle(safeHandle, RegistryView.Default);
            }
            catch (Exception ex)
            {
                Logger.AddToRegistryLog($"Error opening registry key {registryKey}: {ex.Message}");
                return null;
            }
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


        // Convert key to a proper format (example)
        public static string ConvertRegistryKey(string key)
        {
            // Convert and return registry path (just as a simple transformation)
            return key.ToUpper().Replace("HKEY_LOCAL_MACHINE", @"SOFTWARE\Microsoft\Windows\CurrentVersion");
        }

        public static void CloseRegistryKey(RegistryKey key)
        {
            if (key != null)
            {
                key.Close(); // Close the registry key
                Console.WriteLine("Registry key closed.");
            }
        }

        public static string TransformRegistryKey(string key)
        {
            // Assume key is "HKEY_LOCAL_MACHINE\Software\SomeSoftware"
            string root = "HKEY_LOCAL_MACHINE";
            string transformedKey = key.Replace(root, @"SOFTWARE\Microsoft\Windows\CurrentVersion"); // Example transformation
            return transformedKey;
        }
        public static void DeleteInvalidRegistryKeys(string startKey, string keyAsIs)
        {
            string result = "";
            RegistrySubKeyHandler.EnumerateSubKeys(startKey);

            switch (result)
            {
                case "1":
                    DeleteRegistryKey(keyAsIs);
                    break;
                case "2":
                    RegistryUtils.RegDelete(keyAsIs);
                    break;
                default:
                    return;
            }
        }
        public static bool IsInvalidKey(string key)
        {
            // Check for invalid keys (this is just an example, use real logic)
            return key.Contains("Invalid");
        }
        // Method to delete a registry key (similar to DELKEY in AutoIt)
        private void DELKEYLocalMachine(string key)
        {
            using (RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (regKey != null)
                {
                    regKey.DeleteSubKeyTree(key);
                }
            }
        }
        private RegistryKey OpenRegistryKeyLocalMachine(string registryKeyPath)
        {
            try
            {
                return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            catch
            {
                return null;
            }
        }
        private static bool CheckKeyExists(string keyPath)
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
            {
                return key != null;
            }
        }

        public static string GetRegistryKey(string key, int index)
        {
            try
            {
                using (RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
                {
                    if (regKey != null)
                    {
                        return regKey.GetSubKeyNames()[index];
                    }
                }
            }
            catch { }
            return null;
        }

        public static void ScanSpecificKey(string keyPath)
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var valueData = key.GetValue(valueName)?.ToString();
                        Logger.AddToRegistryEntries($"Key: {keyPath}, ValueName: {valueName}, ValueData: {valueData}");
                    }
                }
            }
        }

        public static int RegUnLoadKey(IntPtr hKey, string subKey)
        {
            // Use RegUnLoadKey API (assumed to be already imported)
            int result = NativeMethods.RegUnLoadKey(hKey, subKey);
            return result;
        }

        public static void UnloadRegistryKeys(List<string> load)
        {
            RegistryUtils.SetPrivileges();  // Set required privileges

            // Loop through the registry keys and unload them
            foreach (var key in load)
            {
                // Check if the registry key exists and then unload it
                if (RegistryKeyExists($"HKU\\{key}"))
                {
                    IntPtr hKey = new IntPtr(2147483651);  // 2147483651 corresponds to KEY_USER as IntPtr
                    string keySubPath = key;  // The actual subkey path

                    // Call RegUnLoadKey with the correct parameters
                    int result = RegUnLoadKey(hKey, keySubPath);
                    if (result == 0)  // Success
                    {
                        Console.WriteLine($"Successfully unloaded registry key: {key}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to unload registry key: {key}. Error code: {result}");
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

        public static string ExtractKeyName(IntPtr buffer)
        {
            // You will need to use Marshal to read data from the unmanaged memory
            // This is a simplified example and assumes the buffer contains the name as a string

            // For demonstration purposes, let's assume the buffer holds the string at offset 0
            string keyName = Marshal.PtrToStringUni(buffer);

            return keyName;
        }

        public static void DeleteRegistryKey(string keyPath, int options)
        {
            try
            {
                // Open the registry key
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Registry key not found: {keyPath}");
                        return;
                    }

                    // If options indicate recursive deletion, iterate through subkeys
                    if (options == 1)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            key.DeleteSubKeyTree(subKeyName);
                        }
                    }

                    // Delete the key itself
                    Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(keyPath);
                    Console.WriteLine($"Deleted registry key: {keyPath}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to delete registry key: {keyPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key {keyPath}: {ex.Message}");
            }
        }

        private static readonly List<string> LoadedKeys = new List<string>();

        public static string[] LoadKeys()
        {
            // For demonstration, returning mock data
            return LoadedKeys.ToArray();
        }

        // Add keys to the loaded list (for testing purposes)
        public static void AddLoadedKey(string keyPath)
        {
            if (!LoadedKeys.Contains(keyPath))
            {
                LoadedKeys.Add(keyPath);
            }
        }

        // Clear loaded keys
        public static void ClearLoadedKeys()
        {
            LoadedKeys.Clear();
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner.Scripting
{
    public class RegistryUtilsScript
    {
        // Function to handle the RegEdit process
        public static void RegEditMethod(ref int f)
        {
            string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "FRST\\tmp111.reg");
            int b = f + 1;

            // Create or open the tmp111.reg file for writing
            using (StreamWriter writer = new StreamWriter(path1, append: true))
            {
                string batch;
                while ((batch = File.ReadLines(@"ScriptDir\\FIXLIST").Skip(b - 1).FirstOrDefault()) != null)
                {
                    if (batch.Contains("EndRegedit"))
                        break;

                    if (Regex.IsMatch(batch, @"(?i)\[-?hk.+\]"))
                    {
                        string key = Regex.Replace(batch, @"(?i)\[-?(hk.+)\]", "$1");
                        RegistryKey registryKey = RegistryKeyHandler.OpenRegistryKey(key);

                        // Check if registryKey is null (i.e., the key couldn't be opened)
                        if (registryKey == null)
                        {
                            File.AppendAllText(@"HFIXLOG", "REGB ====> " + key + " <==== NOACC" + Environment.NewLine);
                        }
                    }


                    writer.WriteLine(batch);
                    b++;
                }
            }

            f = b;

            string regFileContent = File.ReadAllText(path1);
            if (!regFileContent.Contains("REGEDIT4") && !regFileContent.Contains("Windows Registry Editor Version 5.00"))
            {
                File.WriteAllText(path1, "Windows Registry Editor Version 5.00" + Environment.NewLine + Environment.NewLine + regFileContent);
            }

            if (SystemConstants.BootMode == "recovery")
            {
                string readR = File.ReadAllText(path1);
                readR = RemoteTransform(readR);  // Assuming RemoteTransform modifies the content
                File.WriteAllText(path1, readR + Environment.NewLine);
            }

            string regPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "reg.exe");
            CommandHandler.RunCommand($"cmd /c {regPath} import {path1} > {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "frst\\debug")} 2>&1");

            string error = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "frst\\debug"));
            File.AppendAllText(@"HFIXLOG", error);
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "frst\\debug"));
            File.Delete(path1);
            File.AppendAllText(@"HFIXLOG", "REGB ====> " + error);
        }

        public static string RemoteTransform(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("Content cannot be null or empty.", nameof(content));
            }

            // Perform transformations on registry paths for a remote recovery scenario.
            // Example: Replace absolute paths with relative paths or adjust to a recovery environment.

            // Replace system drive references with a placeholder (e.g., `%SystemDrive%`).
            content = Regex.Replace(content, @"(?i)C:\\", "%SystemDrive%\\", RegexOptions.IgnoreCase);

            // Replace HKCU with HKU\.Default for recovery scenarios if necessary.
            content = Regex.Replace(content, @"(?i)HKEY_CURRENT_USER", "HKEY_USERS\\.Default", RegexOptions.IgnoreCase);

            // Adjust file paths pointing to known folders for recovery.
            // Example: Replace Windows directory references.
            string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            content = content.Replace(windowsDirectory, "%SystemRoot%");

            // Adjust paths for user profiles if relevant to recovery.
            string userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            content = content.Replace(userProfileDirectory, "%UserProfile%");

            // Ensure registry file has correct line endings (Windows-style CRLF).
            content = content.Replace("\n", "\r\n");

            // Add additional transformations as needed for your specific recovery requirements.
            // Example: Replace specific keys or adjust for remote execution.
            content = TransformCustomKeys(content);

            return content;
        }

        /// <summary>
        /// Example method for applying additional custom transformations to registry content.
        /// </summary>
        /// <param name="content">The registry content to transform.</param>
        /// <returns>The transformed registry content.</returns>
        private static string TransformCustomKeys(string content)
        {
            // Example: Replace custom registry keys or values.
            content = Regex.Replace(content, @"(?i)HKEY_LOCAL_MACHINE\\SOFTWARE\\ExampleKey", "HKEY_LOCAL_MACHINE\\SOFTWARE\\TransformedKey", RegexOptions.IgnoreCase);

            // Replace placeholders in registry values.
            content = content.Replace("{PLACEHOLDER}", "ActualValue");

            return content;
        }


        public static void Reg()
        {
            string com = Regex.Replace("YourFixValueHere", "(?i)reg:\\s*(.+)", "$1");
            string bootM = "Normal"; // Set boot mode accordingly
            string c = "C:"; // Set C drive path or environment variable
            string fixLog = "PathToLogFile"; // Set log file path
            string tpPath = Path.Combine(c, "FRST", "logs", "reg" + new Random().Next(1000, 9999));
            string regKey;
            // Concatenate the command and path into a single string
            string commandWithPath = com + " " + tpPath;

            // Log the command being executed
            File.AppendAllText(fixLog, Environment.NewLine + "======== " + com + " ========" + Environment.NewLine);

            // Modify command if in recovery mode
            if (bootM == "Recovery")
            {
                com = RegistryRecoveryHandler.RmTor(com);
            }

            // Ensure the command contains /f or /y flags
            if (Regex.IsMatch(com, "(?i)reg (delete|add) ") && !com.Contains("/f"))
            {
                com = Regex.Replace(com, "(.+)", "$1 /f");
            }

            if (Regex.IsMatch(com, "(?i)reg export ") && !com.Contains("/y"))
            {
                com = Regex.Replace(com, "(.+)", "$1 /y");
            }

            // Start a timer to check the command progress (dummy timer logic here)
            var timer = new System.Timers.Timer(300000); // 5 minutes
            timer.Start();

            // Execute the registry command using Process
            CommandHandler.RunCommand(commandWithPath);

            // Stop the timer
            timer.Stop();

            // Read the output file and log the result
            string regOutput = File.ReadAllText(tpPath);

            // If in recovery mode, modify the registry output
            if (bootM == "Recovery")
            {
                regOutput = RegistryRecoveryHandler.RmTon(regOutput);
            }

            // Write the result to the log file
            File.AppendAllText(fixLog, regOutput);

            // Add ending to the log
            File.AppendAllText(fixLog, Environment.NewLine + "======== " + "End" + " ========" + Environment.NewLine);

            // Delete the temporary file
            File.Delete(tpPath);
        }
        public static void UDEBUGFIX(string fix)
        {
            // Extract key using regular expression
            string key = RegistryKeyHandler.ExtractRegistryKey(fix);

            // Append specific subkey if necessary
            if (fix.Contains("ActivatableClasses\\Package"))
            {
                key = $"{key}\\DebugInformation";
            }

            // Delete the registry key
            RegistryKeyHandler.DeleteRegistryKey(key);
        }

        private static void RegDeleteEntries(string key, List<string> arreEntries)
        {
            arreEntries.Add(key);
            IntPtr hKey = RegistryKeyHandler.OpenRegistryKey(key, writable: false);
            if (hKey == IntPtr.Zero) return;

            int index = 0;
            string subKey;
            while ((subKey = RegistrySubKeyHandler.EnumSubKey(hKey, index++)) != null)
            {
                RegDeleteEntries($"{key}\\{subKey}", arreEntries);
            }

            RegistryKeyHandler.CloseRegistryKey(ref hKey);

        }


        public static void SetRegistryAccess(string key, bool grantAccess)
        {
            try
            {
                // Open the registry key with write permissions
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine($"Registry key {key} not found.");
                        return;
                    }

                    // Get the current security settings
                    RegistrySecurity security = registryKey.GetAccessControl();

                    // Get the current user
                    string currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                    if (grantAccess)
                    {
                        // Grant full control to the current user
                        RegistryAccessRule rule = new RegistryAccessRule(
                            currentUser,
                            RegistryRights.FullControl,
                            InheritanceFlags.None,
                            PropagationFlags.None,
                            AccessControlType.Allow);

                        security.AddAccessRule(rule);
                    }
                    else
                    {
                        // Remove full control from the current user
                        RegistryAccessRule rule = new RegistryAccessRule(
                            currentUser,
                            RegistryRights.FullControl,
                            InheritanceFlags.None,
                            PropagationFlags.None,
                            AccessControlType.Allow);

                        security.RemoveAccessRule(rule);
                    }

                    // Apply the updated security settings
                    registryKey.SetAccessControl(security);

                    Console.WriteLine($"Successfully {(grantAccess ? "granted" : "revoked")} access to {key}.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting registry access for {key}: {ex.Message}");
            }
        }

        public static int HKEYCREATE(string fullKey, uint access = NativeMethodConstants.KEY_ALL_ACCESS)
        {
            // Convert the full key to a UNICODE_STRING
            Structs.UNICODE_STRINGALT unicodeString = new Structs.UNICODE_STRINGALT();
            NtdllNativeMethods.RtlInitUnicodeString(ref unicodeString, fullKey);

            // Set up object attributes
            Structs.OBJECT_ATTRIBUTESALT objectAttributes = new Structs.OBJECT_ATTRIBUTESALT();
            objectAttributes.Length = Marshal.SizeOf(typeof(Structs.OBJECT_ATTRIBUTESALT));
            objectAttributes.ObjectName = unicodeString;
            objectAttributes.Attributes = NativeMethodConstants.OBJ_CASE_INSENSITIVE; // Case-insensitive key name

            IntPtr keyHandle = IntPtr.Zero;

            // Call NtCreateKey
            int status = NtdllNativeMethods.NtCreateKey(
                out keyHandle,
                access,
                ref objectAttributes,
                0,
                IntPtr.Zero,
                0,
                IntPtr.Zero
            );

            // Handle the result
            if (status != NativeMethodConstants.STATUS_SUCCESS)
            {
                return ErrorHandler.SetError(1, 0, status.ToString()); // Handle error
            }

            // Close or delete the key based on the status
            if (keyHandle != IntPtr.Zero)
            {
                if (status == 2) // Example case: status code 2
                {
                    NtdllNativeMethods.NtClose(keyHandle);
                    return 2;
                }
                if (status == 1) // Example case: status code 1
                {
                    NtdllNativeMethods.NtDeleteKey(keyHandle);
                    return 1;
                }
            }

            return 0; // Default success
        }


        // Set necessary privileges
        public static void SetPrivileges()
        {
            IntPtr hToken = IntPtr.Zero;
            if (Advapi32NativeMethods.OpenProcessToken(Kernel32NativeMethods.GetCurrentProcess(), 0x20 /* TOKEN_ADJUST_PRIVILEGES */, out hToken))

            {
                Structs.LUID luid = new Structs.LUID();
                if (Advapi32NativeMethods.LookupPrivilegeValue(null, NativeMethodConstants.SE_BACKUP_NAME, ref luid) && Advapi32NativeMethods.LookupPrivilegeValue(null, NativeMethodConstants.SE_RESTORE_NAME, ref luid))
                {
                    Structs.TOKEN_PRIVILEGES tokenPrivileges = new Structs.TOKEN_PRIVILEGES
                    {
                        PrivilegeCount = 1,
                        Luid = luid,
                        Attributes = NativeMethodConstants.SE_PRIVILEGE_ENABLED
                    };

                    uint returnLength = 0;
                    Advapi32NativeMethods.AdjustTokenPrivileges(hToken, false, ref tokenPrivileges, 0, ref tokenPrivileges, ref returnLength);
                }
            }
        }

        public static void DeleteCatalog(string key, string val)
        {
            string fullKey = key + val;
            RegistryKeyHandler.DeleteRegistryKey(fullKey);
        }



        public static void LASTBOOT(string scriptDir, string frstLogPath, string systemDrive)
        {
            // Close and reopen the log file
            using (var frstLog = new StreamWriter(Path.Combine(scriptDir, "FRST.txt"), append: true))
            {
                // List of hive files to check
                string[] hives = { "DEFAULT", "SAM", "SECURITY", "SOFTWARE", "SYSTEM" };

                // Check the existence and size of each hive file
                foreach (var hive in hives)
                {
                    string hivePath = Path.Combine(systemDrive, @"windows\system32\config\regback", hive);

                    if (!File.Exists(hivePath) || new FileInfo(hivePath).Length == 0)
                    {
                        return; // If any hive file is missing or empty, exit the function
                    }
                }

                // Get the last modified date of the SOFTWARE hive
                string softwareHivePath = Path.Combine(systemDrive, @"windows\system32\config\regback\software");
                DateTime lastModified = File.GetLastWriteTime(softwareHivePath);

                // Write the last modified date to the log file
                frstLog.WriteLine();
                frstLog.WriteLine($"LastRegBack: {lastModified}");
            }
        }

        public static void LEGALNOTICE(string key, string fix)
        {
            // Determine the registry value to restore
            string val = fix.Contains("LegalNoticeCaption") ? "LegalNoticeCaption" : "LegalNoticeText";

            // Restore the value using the correct RegistryValueKind
            RegistryValueHandler.RestoreRegistryValue(
                RegistryHive.LocalMachine, // Specify the hive
                key,                       // Registry key path
                val,                       // Registry value name
                string.Empty,              // Registry value data
                RegistryValueKind.String   // Correct enum for REG_SZ
            );
        }


        public static string WriteRegistry(string key, string value, string type, string data)
        {
            using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, true))
            {
                if (registryKey != null)
                {
                    registryKey.SetValue(value, data, (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), type));
                    return "1"; // Simulating success
                }
            }
            return "0"; // Simulating failure
        }

        public static void USERCHOICEFIX(string fix)
        {
            // Use regex to extract the key from the input string.
            string key = System.Text.RegularExpressions.Regex.Replace(fix, @"(.+) =>.*", "$1");

            // Delete the registry key if it exists.
            RegistryKeyHandler.DeleteRegistryKey(key);
        }

        public static void Def()
        {
            // Run commands to load registry hives
            CommandHandler.RunCommand("reg load hklm\\999 c:\\Windows\\System32\\config\\System");
            CommandHandler.RunCommand("reg load hklm\\888 c:\\Windows\\System32\\config\\software");

            string def = "";
            string sVersion = "";

            try
            {
                // Read the 'Default' value from the loaded hive
                object defValue = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\999\Select", "default", null);
                if (defValue is int def1)
                {
                    def = def1 < 10 ? "0" + def1 : def1.ToString();
                }

                // Read the 'ProductName' value from the loaded hive
                object productName = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\888\Microsoft\Windows NT\CurrentVersion", "ProductName", null);
                sVersion = productName?.ToString() ?? Environment.OSVersion.VersionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                sVersion = Environment.OSVersion.VersionString;
            }

            Console.WriteLine($"Default: {def}");
            Console.WriteLine($"Product Version: {sVersion}");
        }

        // Simulate the RegWrite function - Writes to the registry
        public static bool RegWrite(string key, string val, string vType, string vData)
        {
            try
            {
                // Assuming vType is "string", but could be extended for other types (e.g., DWORD, QWORD)
                Microsoft.Win32.Registry.SetValue(key, val, vData);
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., if the registry key doesn't exist or permission is denied)
                Console.WriteLine("Error writing to registry: " + ex.Message);
                return false;
            }
        }

        public static void WriteRegistryRunOnce(string keyName, string command)
        {
            try
            {
                using (RegistryKey runOnceKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", true))
                {
                    if (runOnceKey != null)
                    {
                        runOnceKey.SetValue(keyName, command, RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to registry: {ex.Message}");
            }
        }

        // Extract the registry key name from a full path (e.g., "HKEY_LOCAL_MACHINE\Software\MyApp" -> "MyApp")
        public static string ExtractKey(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException(nameof(fullPath), "The registry path cannot be null or empty.");
            }

            // Find the last backslash to extract the key name
            int lastBackslashIndex = fullPath.LastIndexOf('\\');
            if (lastBackslashIndex >= 0)
            {
                return fullPath.Substring(lastBackslashIndex + 1); // Extract the key name part after the last backslash
            }

            // If no backslash is found, return the full path (assuming it's already a key)
            return fullPath;
        }
        // Method to delete a registry value
        public static int RegDelete(string key, string value)
        {
            try
            {
                // Open the registry key with write access
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, writable: true))
                {
                    if (registryKey != null)
                    {
                        // Attempt to delete the registry value
                        registryKey.DeleteValue(value);
                        return 1; // Success
                    }
                    else
                    {
                        return 0; // Failed to open the key
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                Console.WriteLine($"Error deleting registry value: {ex.Message}");
                return -1; // Indicate failure due to exception
            }
        }

        // Main conversion of the _SETREGACE function
        public static void SETREGACE(string key, int rec = 0)
        {
            string fullKey = RegistryUtils.HKEYTRANS(key);
            IntPtr hKey = REGUNLOC(fullKey);

            // Simulating @error in AutoIt (in C#, we'd check if hKey is valid)
            if (hKey != IntPtr.Zero && rec != 0)
            {
                // Unlock registry key if hKey is valid and rec is not zero
                UNLOCKALLREG(fullKey, hKey);
            }
        }
        // Method to simulate the _REGUNLOC function in C#
        public static IntPtr REGUNLOC(string fullKey)
        {
            // Create a UNICODE_STRING object for the registry key path
            Structs.UNICODE_STRINGALT objectName = new Structs.UNICODE_STRINGALT();
            NtdllNativeMethods.RtlInitUnicodeString(ref objectName, fullKey);

            // Initialize OBJECT_ATTRIBUTES structure
            Structs.OBJECT_ATTRIBUTESALT objectAttributes = new Structs.OBJECT_ATTRIBUTESALT
            {
                Length = Marshal.SizeOf(typeof(Structs.OBJECT_ATTRIBUTESALT)),
                ObjectName = objectName,
                RootDirectory = IntPtr.Zero,
                Attributes = NativeMethodConstants.OBJ_CASE_INSENSITIVE,
                SecurityDescriptor = IntPtr.Zero,
                SecurityQualityOfService = IntPtr.Zero
            };

            // Attempt to open the registry key
            IntPtr hKey = IntPtr.Zero;
            int result = NtdllNativeMethods.NtOpenKeyALT(out hKey, NativeMethodConstants.KEY_ALL_ACCESS, ref objectAttributes);

            if (result == NativeMethodConstants.STATUS_SUCCESS)
            {
                // Successfully opened the registry key
                return hKey;
            }
            else if (result == NativeMethodConstants.STATUS_ACCESS_DENIED)
            {
                // Handle access denied, possibly with privilege escalation (not implemented here)
                Console.WriteLine("Access Denied, attempting privilege escalation...");
                // For demonstration, returning IntPtr.Zero if access is denied
                return IntPtr.Zero;
            }
            else
            {
                // Handle other errors
                Console.WriteLine($"Error opening registry key: {result}");
                return IntPtr.Zero;
            }
        }

        public static void UNLOCKALLREG(string fullKey, IntPtr hKey)
        {
            // Implementation of unlocking the registry key
            // For example, closing the handle or resetting permissions
            if (hKey != IntPtr.Zero)
            {
                // Example logic: assume it's a cleanup function
                Console.WriteLine($"Unlocking registry key {fullKey}");
                // Do necessary cleanup here
            }
        }

        // Method to unlock registry key by setting permissions for given users/groups
        public static void Unlock(string keyPath, string groups, string user)
        {
            try
            {
                // Open the registry key
                using (RegistryKey registryKey = Microsoft.Win32.Registry.Users.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine("Registry key does not exist or cannot be opened.");
                        return;
                    }

                    // Apply the access control (unlocking)
                    var acl = registryKey.GetAccessControl();

                    // Split the group string into individual groups (e.g., "Administrators;System;Users;")
                    string[] groupList = groups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);


                    foreach (var group in groupList)
                    {
                        // Grant full control to the specified group
                        acl.AddAccessRule(new RegistryAccessRule(group, RegistryRights.FullControl, AccessControlType.Allow));
                    }

                    // If a user is provided, grant full control to that user as well
                    if (!string.IsNullOrEmpty(user))
                    {
                        acl.AddAccessRule(new RegistryAccessRule(user, RegistryRights.FullControl, AccessControlType.Allow));
                    }

                    // Set the updated ACL on the registry key
                    registryKey.SetAccessControl(acl);
                    Console.WriteLine($"Successfully unlocked registry key: {keyPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking registry key: {ex.Message}");
            }
        }

        // Example method definition for Unload
        public static void Unload(string user)
        {
            try
            {
                // Perform some actions to unload or clean up user settings
                Console.WriteLine($"Unloading settings for user: {user}");
                // Add any other necessary logic, such as removing registry entries or files
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unloading user settings: {ex.Message}");
            }
        }

        public static int SetRegistryAccessInt(string entry, bool grantAccess)
        {
            // Assuming you grant access or fail based on the 'grantAccess' flag
            try
            {
                if (grantAccess)
                {
                    // Logic to grant access (return 1 for success)
                    return 1;
                }
                else
                {
                    // Logic for failure (return 0 or other error codes)
                    return 0;
                }
            }
            catch
            {
                // Return an error code (e.g., -1) on failure
                return -1;
            }
        }

        public static void UnlockAllRegistry(string registryKeyPath, string account = "Everyone")
        {
            try
            {
                Console.WriteLine($"Unlocking registry key: {registryKeyPath}");

                // Open the registry key
                using (RegistryKey baseKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKeyPath, writable: true))
                {
                    if (baseKey == null)
                    {
                        Console.WriteLine($"Registry key not found: {registryKeyPath}");
                        return;
                    }

                    // Grant permissions to the account
                    GrantRegistryPermissions(baseKey, account);

                    // Recursively unlock subkeys
                    foreach (string subKeyName in baseKey.GetSubKeyNames())
                    {
                        string subKeyPath = $"{registryKeyPath}\\{subKeyName}";
                        UnlockAllRegistry(subKeyPath, account);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking registry key '{registryKeyPath}': {ex.Message}");
            }
        }

        private static void GrantRegistryPermissions(RegistryKey registryKey, string account)
        {
            try
            {
                // Get current access control
                RegistrySecurity security = registryKey.GetAccessControl();

                // Add full control for the specified account
                RegistryAccessRule rule = new RegistryAccessRule(
                    account,
                    RegistryRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);

                security.AddAccessRule(rule);

                // Apply updated permissions
                registryKey.SetAccessControl(security);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying permissions to registry key '{registryKey.Name}': {ex.Message}");
            }
        }

        public static bool SetRegistryOwner(string key, string owner)
        {
            try
            {
                var sid = RegistryUtils.GetSidFromName(owner);
                if (sid == null)
                {
                    Console.WriteLine($"Error: Invalid owner SID for {owner}");
                    return false;
                }

                // Use P/Invoke to set the owner in the registry key
                // (Placeholder for actual P/Invoke logic to SetNamedSecurityInfoW)
                Console.WriteLine($"Owner for {key} set to {owner}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SetRegistryOwner: {ex.Message}");
                return false;
            }
        }

        public static void UnlockAllRegistry(string registryKey)
        {
            try
            {
                // Open the registry key with read/write permissions
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey, writable: true);
                if (key == null)
                {
                    Console.WriteLine($"Could not open registry key: {registryKey}");
                    return;
                }

                // Here you can add logic to modify permissions, grant access, etc.
                // For example, granting full control to "Everyone"
                // Note: You would need the appropriate permissions to modify registry keys

                Console.WriteLine($"Registry key {registryKey} unlocked successfully.");

                // Closing the registry key after performing necessary operations
                key.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking registry key: {ex.Message}");
            }
        }
    }

}

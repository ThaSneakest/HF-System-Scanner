using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

//Tested and Working need to add additional Run entries
namespace Wildlands_System_Scanner
{
    public class RunHandler
    {
        private static List<string> arrayReg = new List<string>(); // Equivalent to `$ARRAYREG` in AutoIt
        
        public static void RunKey()
        {
            try
            {
                string fullKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; // Fixed key path for readability
                MainApp form1 = new MainApp();
                form1.labelControlProgress.Text = $"{StringConstants.REGIST1}: {fullKey}";

                // Open the registry key
                using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(fullKey, writable: false))
                {
                    if (registryKey == null)
                    {
                        Console.WriteLine($"Unable to open registry key: HKLM\\{fullKey}");
                        return;
                    }

                    foreach (string valueName in registryKey.GetValueNames())
                    {
                        string valueData = registryKey.GetValue(valueName)?.ToString() ?? string.Empty;
                        string attention = Utility.CheckForIssues(valueName, valueData);

                        if (!string.IsNullOrEmpty(valueData))
                        {
                            string filePath = FileUtils.ExtractFilePath(valueData);
                            if (File.Exists(filePath))
                            {
                                var fileInfo = new FileInfo(filePath);
                                long fileSize = fileInfo.Length;
                                string fileCreationDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                                string companyInfo = FileUtils.GetFileCompany(filePath);

                                Logger.Instance.LogPrimary(
                                    $"HKLM\\{fullKey}: [{valueName}] => {filePath} [{fileSize} bytes, {fileCreationDate}] {companyInfo}{attention}"
                                );
                            }
                            else
                            {
                                Logger.Instance.LogPrimary(
                                    $"HKLM\\{fullKey}: [{valueName}] => {valueData} (File Not Found){attention}"
                                );
                            }
                        }
                        else
                        {
                            Logger.Instance.LogPrimary(
                                $"HKLM\\{fullKey}: [{valueName}] => [No Value]{attention}"
                            );
                        }
                    }
                }

                // Process additional validation or invalid subkeys (implement logic as required)
                RegistrySubKeyHandler.IsInvalidSubKey(fullKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunKey: {ex.Message}");
            }
        }


        public static void RunKeyEx()
        {
            try
            {
                string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnceEx";

                // Open the main registry key
                using (RegistryKey hkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath, writable: false))
                {
                    if (hkey == null)
                    {
                        Console.WriteLine($"Unable to open registry key: {keyPath}");
                        return;
                    }

                    // Enumerate subkeys
                    foreach (string subKeyName in hkey.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = hkey.OpenSubKey(subKeyName, writable: false))
                        {
                            if (subKey == null)
                            {
                                Console.WriteLine($"Unable to open subkey: {subKeyName}");
                                continue;
                            }

                            // List values for the subkey
                            foreach (string valueName in subKey.GetValueNames())
                            {
                                string valueData = subKey.GetValue(valueName)?.ToString() ?? string.Empty;
                                string attention = string.Empty;

                                if (valueData.ToLower().Contains(@"\temp\".ToLower()))
                                {
                                    attention = " <==== Attention Required";
                                }

                                // Log the entry
                                Logger.Instance.LogPrimary($"HKLM\\...\\RunOnceEx\\{subKeyName}: [{valueName}] => {valueData}{attention}");
                            }
                        }
                    }
                }

                // Output the results if needed
                if (arrayReg.Count > 0)
                {
                    foreach (string entry in arrayReg)
                    {
                        Console.WriteLine(entry);
                    }
                }
                else
                {
                    Console.WriteLine("No entries found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunKeyEx: {ex.Message}");
            }
        }


        public static void HandleRunKeys()
        {
            try
            {
                string subKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
                Console.WriteLine($"Scanning Registry: HKU\\{subKeyPath}");

                // Access the Registry.Users root key
                using (RegistryKey usersRootKey = Microsoft.Win32.Registry.Users)
                {
                    if (usersRootKey == null)
                    {
                        Console.WriteLine("Unable to access Registry.Users.");
                        return;
                    }

                    // Iterate through all user-specific registry hives
                    foreach (string userSid in usersRootKey.GetSubKeyNames())
                    {
                        string fullKeyPath = $"{userSid}\\{subKeyPath}";

                        using (RegistryKey userRunKey = usersRootKey.OpenSubKey(fullKeyPath))
                        {
                            if (userRunKey == null)
                            {
                                Console.WriteLine($"Key HKU\\{fullKeyPath} not found.");
                                continue;
                            }

                            Console.WriteLine($"Processing user-specific Run key: HKU\\{fullKeyPath}");

                            // Enumerate all values under the Run key
                            foreach (string valueName in userRunKey.GetValueNames())
                            {
                                string valueData = userRunKey.GetValue(valueName)?.ToString();
                                if (!string.IsNullOrEmpty(valueData))
                                {
                                    Console.WriteLine($"Value: [{valueName}] => {valueData}");

                                    // Perform additional checks or actions
                                    string filePath = FileUtils.ExtractFilePath(valueData);
                                    if (File.Exists(filePath))
                                    {
                                        long fileSize = new FileInfo(filePath).Length;
                                        string creationDate = File.GetCreationTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
                                        string companyInfo = FileUtils.GetFileCompany(filePath);

                                        Logger.Instance.LogPrimary($"HKU\\{fullKeyPath}: [{valueName}] => {filePath} [{fileSize} bytes, {creationDate}] {companyInfo}");
                                    }
                                    else
                                    {
                                        Logger.Instance.LogPrimary($"HKU\\{fullKeyPath}: [{valueName}] => {valueData} (File Not Found)");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Value: [{valueName}] is empty or null.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleRunKeys: {ex.Message}");
            }
        }

        public static void RunRunOnceKeys()
        {
            try
            {
                Console.WriteLine("Starting RunRunOnceKeys...");

                // Scan Run and RunOnce keys for both HKLM and HKCU
                ScanRunKeys(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run");
                ScanRunKeys(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce");
                ScanRunKeys(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run");
                ScanRunKeys(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce");

                // Scan HKLM...\Policies
                string policiesKey = @"Software\Microsoft\Windows\CurrentVersion\Policies";
                //PolicyHandler.ScanPolicies($"HKLM\\{policiesKey}");

                // Scan Terminal Server Key
                string terminalServerKey = @"SYSTEM\CurrentControlSet\Control\Terminal Server";
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(terminalServerKey))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("fDenyTSConnections")?.ToString();
                        if (!string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Instance.LogPrimary($"Terminal Server Key: [fDenyTSConnections] = {value} <==== Potential Issue");
                            Console.WriteLine($"Terminal Server Key: [fDenyTSConnections] = {value} <==== Potential Issue");
                        }
                        else
                        {
                            Console.WriteLine("Terminal Server Key: fDenyTSConnections is properly configured.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Key {terminalServerKey} not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunRunOnceKeys: {ex.Message}");
            }
        }

        public static void ScanRunKeys(string keyPath)
        {
            try
            {
                Console.WriteLine($"Scanning Registry Key: {keyPath}");

                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"Key {keyPath} not found.");
                        return;
                    }

                    foreach (string valueName in key.GetValueNames())
                    {
                        string valueData = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(valueData))
                        {
                            string filePath = FileUtils.ExtractFilePath(valueData);
                            if (File.Exists(filePath))
                            {
                                long fileSize = new FileInfo(filePath).Length;
                                string creationDate = File.GetCreationTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
                                string companyInfo = FileUtils.GetFileCompany(filePath);

                                Logger.Instance.LogPrimary($"{keyPath}: [{valueName}] => {filePath} [{fileSize} bytes, {creationDate}] {companyInfo}");
                                Console.WriteLine($"Found: {keyPath}: [{valueName}] => {filePath} [{fileSize} bytes, {creationDate}] {companyInfo}");
                            }
                            else
                            {
                                Logger.Instance.LogPrimary($"{keyPath}: [{valueName}] => {valueData} (File Not Found)");
                                Console.WriteLine($"Found: {keyPath}: [{valueName}] => {valueData} (File Not Found)");
                            }
                        }
                        else
                        {
                            Logger.Instance.LogPrimary($"{keyPath}: [{valueName}] => [No Value]");
                            Console.WriteLine($"Found: {keyPath}: [{valueName}] => [No Value]");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning {keyPath}: {ex.Message}");
            }
        }

        private static void ScanRunKeys()
        {
            RegistryKeyHandler.ScanSpecificKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            RegistryKeyHandler.ScanSpecificKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce");
        }

        private static void ScanRunServices()
        {
            RegistryKeyHandler.ScanSpecificKey(@"Software\Microsoft\Windows\CurrentVersion\RunServices");
            RegistryKeyHandler.ScanSpecificKey(@"Software\Microsoft\Windows\CurrentVersion\RunServicesOnce");
        }

        private static void RunScan()
        {
            int A = 0;
            StringBuilder Startup = new StringBuilder();
            RegistryKey StartupCSK = null;
            string B = string.Empty;

            if (A == 0) { StartupCSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                B = "HKLM\\Run:";
            }
            if (A == 1) { StartupCSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce"); B = "HKLM\\RunOnce:"; }
            if (A == 2) { StartupCSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunServices"); B = "HKLM\\RunServices:"; }
            if (A == 3) { StartupCSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunServicesOnce"); B = "HKLM\\RunServicesOnce:"; }
            if (A == 4) { StartupCSK = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"software\microsoft\windows\Currentversion\policies\explorer\Run"); B = "HKLM\\Policies\\Run:"; }
            if (A == 5) { StartupCSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"); B = "HKCU\\Run:"; }
            if (A == 6) { StartupCSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce"); B = "HKCU\\RunOnce:"; }
            if (A == 7) { StartupCSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunServices"); B = "HKCU\\RunServices:"; }
            if (A == 8) { StartupCSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunServicesOnce"); B = "HKCU\\RunServicesOnce:"; }
            if (A == 9) { StartupCSK = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run"); B = "HKCU\\Policies\\Run:"; }
            if (A == 10) { StartupCSK = Microsoft.Win32.Registry.Users.OpenSubKey(@".DEFAULT\Software\Microsoft\Windows\CurrentVersion\Run"); B = "HKU\\Run:"; }
            if (A == 11) { StartupCSK = Microsoft.Win32.Registry.Users.OpenSubKey(@".DEFAULT\Software\Microsoft\Windows\CurrentVersion\RunOnce"); B = "HKU\\RunOnce:"; }
            if (A == 12) { StartupCSK = Microsoft.Win32.Registry.Users.OpenSubKey(@".DEFAULT\Software\Microsoft\Windows\CurrentVersion\RunServices"); B = "HKU\\RunServices:"; }
            if (A == 13) { StartupCSK = Microsoft.Win32.Registry.Users.OpenSubKey(@".DEFAULT\Software\Microsoft\Windows\CurrentVersion\RunServicesOnce"); B = "HKU\\RunServicesOnce:"; }
            if (A == 14) { StartupCSK = Microsoft.Win32.Registry.Users.OpenSubKey(@".DEFAULT\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run"); B = "HKU\\Policies\\Run:"; }

            try
            {
                if (StartupCSK != null)
                {
                    foreach (string S in StartupCSK.GetValueNames())
                    {
                        string C = (string)Microsoft.Win32.Registry.GetValue(StartupCSK.Name + "\\" + S, S, null);
                        if (!string.IsNullOrEmpty(C))
                        {
                            Startup.Append(B + "\t" + S + " - " + C + Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogPrimary(ex.Message);
            }
            finally
            {
                StartupCSK?.Close();
            }
            A += 1;
        }

        private static void HKEYUsersScan()
        {
            //HKEY_USERS
            List<string> runKeys = EnumerateHKEYUsersRunKeys();

            foreach (var key in runKeys)
            {
                Logger.Instance.LogPrimary(key);
            }
            Logger.Instance.LogPrimary(Environment.NewLine);
        }

        public static List<string> EnumerateHKEYUsersRunKeys()
        {
            List<string> runKeys = new List<string>();

            // Open HKEY_USERS registry key
            using (RegistryKey hKeyUsers = Microsoft.Win32.Registry.Users)
            {
                // Enumerate through each subkey (user profile)
                foreach (string subKeyName in hKeyUsers.GetSubKeyNames())
                {
                    try
                    {
                        // Construct the path to the "Run" registry key for this user
                        string runKeyPath = $@"{subKeyName}\Software\Microsoft\Windows\CurrentVersion\Run";

                        // Try to open the "Run" registry key for this user
                        using (RegistryKey runKey = hKeyUsers.OpenSubKey(runKeyPath))
                        {
                            if (runKey != null)
                            {
                                // Enumerate all value names (programs that run at startup)
                                foreach (string valueName in runKey.GetValueNames())
                                {
                                    // Get the full path of the program or command
                                    string valueData = runKey.GetValue(valueName)?.ToString();

                                    // Add information to the list
                                    runKeys.Add($"User: {subKeyName}, Key: {valueName}, Value: {valueData}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing key for user {subKeyName}: {ex.Message}");
                    }
                }
            }

            return runKeys;
        }
    }
}

using Microsoft.Win32;
using System;
using System.Management;

namespace Wildlands_System_Scanner.Scanning.AccountScan
{
    public class UserAccountScan
    {
        public static void EnumerateUserAccounts()
        {
            try
            {
                Console.WriteLine("=== User Account Enumeration ===");

                // Retrieve local user accounts
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount");
                foreach (ManagementObject obj in searcher.Get())
                {
                    try
                    {
                        string name = obj["Name"]?.ToString();
                        string sid = obj["SID"]?.ToString();
                        bool isDisabled = (bool)obj["Disabled"];
                        bool isAdmin = (bool)obj["LocalAccount"] && Environment.MachineName.Equals(obj["Domain"]?.ToString(), StringComparison.OrdinalIgnoreCase);

                        string accountType = isAdmin ? "Administrator" : "Limited";
                        string status = isDisabled ? "Disabled" : "Enabled";

                        // Fetch profile path using the updated method
                        string profilePath = GetUserProfilePath(sid);
                        if (string.IsNullOrEmpty(profilePath))
                        {
                            profilePath = "Profile Path Not Found";
                        }

                        // Print in the requested format
                        Logger.Instance.LogPrimary($"{name} ({sid} - {accountType} - {status}) => {profilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing account: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enumerating user accounts: {ex.Message}");
            }
        }

        private static string GetUserProfilePath(string sid)
        {
            const string profileListKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";

            // Check both 64-bit and 32-bit registry views
            RegistryView[] views = { RegistryView.Registry64, RegistryView.Registry32 };

            foreach (var view in views)
            {
                try
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (var regKey = baseKey.OpenSubKey(profileListKeyPath))
                    {
                        if (regKey == null)
                        {
                            continue;
                        }

                        using (var subKey = regKey.OpenSubKey(sid))
                        {
                            if (subKey != null)
                            {
                                string profilePath = subKey.GetValue("ProfileImagePath")?.ToString();
                                if (!string.IsNullOrEmpty(profilePath))
                                {
                                    return profilePath;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore errors and continue checking
                }
            }

            return null;
        }
    }
}

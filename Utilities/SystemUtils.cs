using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;

namespace Wildlands_System_Scanner.Utilities
{
    public class SystemUtils
    {
        
        public static void GetServicePack()
        {
            if (SystemConstants.OperatingSystemVersion.Contains("Windows XP") || SystemConstants.OperatingSystemVersion.Contains("Windows Vista"))
            {
                SystemConstants.ServicePack = (string)Microsoft.Win32.Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                    "CSDVersion",
                    null);

                Logger.Instance.LogPrimary("Operating System: " + SystemConstants.OperatingSystemVersion + " " + SystemConstants.ServicePack);
            }

            if (SystemConstants.OperatingSystemVersion.Contains("Windows 7") || SystemConstants.OperatingSystemVersion.Contains("Windows 8"))
            {
                Logger.Instance.LogPrimary("Operating System: " + SystemConstants.OperatingSystemVersion);
            }
        }

        public static string GetBootMode()
        {
            // Here we check the system environment or a registry key to determine the boot mode
            string bootMode = "Normal"; // Default to normal mode

            // Example logic to check if the system is in recovery mode
            if (System.Environment.GetEnvironmentVariable("OS_RECOVERY_MODE") == "1")
            {
                bootMode = "Recovery"; // Modify as per actual recovery mode check logic
            }

            return bootMode;
        }

        public static string GetOperatingSystem()
        {
            string osName = string.Empty;
            string version = string.Empty;
            string buildNumber = string.Empty;
            string edition = GetWindowsEdition(); // Get edition from registry

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        osName = obj["Caption"]?.ToString() ?? "Unknown OS";
                        version = obj["Version"]?.ToString() ?? "Unknown Version";
                        buildNumber = obj["BuildNumber"]?.ToString() ?? "Unknown Build";
                    }
                }
            }
            catch
            {
                osName = RuntimeInformation.OSDescription;
            }

            // Combine OS name, version, build number, and edition
            return $"{osName} {edition} Version {version} Build {buildNumber}";
        }

        public static string GetWindowsEdition()
        {
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        object edition = key.GetValue("EditionID");
                        return edition != null ? edition.ToString() : "Unknown Edition";
                    }
                }
            }
            catch
            {
                // Fallback in case of any error
                return "Unknown Edition";
            }

            return "Unknown Edition";
        }

        public static string GetTotalPhysicalMemory()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalMemoryBytes = (ulong)obj["TotalPhysicalMemory"];
                        return $"{(totalMemoryBytes / 1024.0 / 1024 / 1024):F2} GB";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error retrieving total memory: {ex.Message}";
            }

            return "Unknown";
        }

        public static string GetAvailablePhysicalMemory()
        {
            try
            {
                using (PerformanceCounter pc = new PerformanceCounter("Memory", "Available Bytes"))
                {
                    float availableMemoryInBytes = pc.RawValue;
                    return $"{(availableMemoryInBytes / 1024 / 1024 / 1024):#0.00} GB";
                }
            }
            catch
            {
                return "Error retrieving available physical memory";
            }
        }

        public static void LogDrives()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady) // Ensure the drive is ready to prevent exceptions
                {
                    string name = string.IsNullOrEmpty(drive.VolumeLabel) ? "Unknown" : drive.VolumeLabel;
                    string letter = drive.Name;
                    string type = drive.DriveFormat;
                    double totalSize = drive.TotalSize / 1024.0 / 1024 / 1024; // Convert bytes to GB
                    double freeSpace = drive.TotalFreeSpace / 1024.0 / 1024 / 1024; // Convert bytes to GB

                    Logger.Instance.LogPrimary($"Name: {name} | Letter: {letter} | Type: {type} | Total: {totalSize:#0.00} GB | Free: {freeSpace:#0.00} GB");
                }
            }
        }

        public static string GetDomain()
        {
            try
            {
                // Create a ManagementObjectSearcher object to query WMI
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");

                // Execute the query and get the results
                ManagementObjectCollection queryCollection = searcher.Get();

                // Loop through the results and check if the computer is part of a domain
                foreach (ManagementObject queryObj in queryCollection)
                {
                    bool partOfDomain = Convert.ToBoolean(queryObj["PartOfDomain"]);
                    if (partOfDomain)
                    {
                        // Return the domain name if part of a domain
                        return queryObj["Domain"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching domain information: " + ex.Message);
            }

            // Return null if not part of a domain or an error occurred
            return null;
        }

        /// <summary>
        /// Checks if the current user has administrative privileges.
        /// </summary>
        /// <returns>True if the current user is an administrator, otherwise false.</returns>
        public static bool IsAdmin()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking administrative privileges: {ex.Message}");
                return false;
            }
        }

    }
}

using System;
using System.Management;

public class VolumeEncryptionHandler
{
    public static int CheckProtectionStatus(string driveLetter, bool checkLockStatus = false)
    {
        int lockStatus = -1; // Return -1 if not found or error

        try
        {
            // Initialize the WMI query for the volume encryption class
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                @"\\.\root\CIMV2\Security\MicrosoftVolumeEncryption",
                $"SELECT * FROM Win32_EncryptableVolume WHERE DriveLetter='{driveLetter}'"
            );

            // Execute the query
            foreach (ManagementObject volume in searcher.Get())
            {
                // Check if we need to check lock status or protection status
                if (checkLockStatus)
                {
                    volume.InvokeMethod("GetLockStatus", null);
                }
                else
                {
                    volume.InvokeMethod("GetProtectionStatus", null);
                }

                // You would typically get the status using properties from WMI,
                // assuming these methods return the appropriate values for lock status
                // and protection status.
                lockStatus = Convert.ToInt32(volume["LockStatus"]); // Adjust this as per actual WMI data available
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckProtectionStatus: {ex.Message}");
        }

        return lockStatus;
    }

    public static string GetSystemInfo(string className)
    {
        string output = string.Empty;

        try
        {
            // Initialize the WMI query for the specified class
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                @"\\.\root\cimv2",
                $"SELECT * FROM {className}"
            );

            // Execute the query and iterate over the results
            foreach (ManagementObject obj in searcher.Get())
            {
                if (className.Equals("Win32_BaseBoard", StringComparison.OrdinalIgnoreCase))
                {
                    // For Win32_BaseBoard, get Manufacturer and Product
                    output = $"{obj["Manufacturer"]} {obj["Product"]}";
                }
                else
                {
                    // For other classes, get Manufacturer, BIOS Version, and ReleaseDate
                    string releaseDate = obj["ReleaseDate"]?.ToString();
                    if (releaseDate != null)
                    {
                        releaseDate = System.Text.RegularExpressions.Regex.Replace(releaseDate, @"(\d{4})(\d{2})(\d{2}).*", "$2/$3/$1");
                    }

                    output = $"{obj["Manufacturer"]} {obj["BIOSVersion"]} {releaseDate}";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetSystemInfo: {ex.Message}");
        }

        return output;
    }
}

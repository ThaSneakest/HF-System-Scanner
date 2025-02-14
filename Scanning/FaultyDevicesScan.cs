using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning
{
    public class FaultyDevicesScan
    {
        public static void EnumerateFaultyDevices()
        {
            try
            {

                // Connect to the WMI namespace
                ManagementScope scope = new ManagementScope(@"\\.\root\cimv2");
                scope.Connect();

                // Query for faulty devices
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode != 0");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject device in searcher.Get())
                    {
                        // Extract details
                        string name = device["Name"]?.ToString() ?? "Unknown";
                        string description = device["Description"]?.ToString() ?? "Unknown";
                        string classGuid = device["ClassGuid"]?.ToString() ?? "Unknown";
                        string manufacturer = device["Manufacturer"]?.ToString() ?? "Unknown";
                        string service = device["Service"]?.ToString() ?? "Unknown";
                        int errorCode = Convert.ToInt32(device["ConfigManagerErrorCode"]);

                        // Get problem and resolution
                        string problem = GetProblemDescription(errorCode);
                        string resolution = GetProblemResolution(errorCode);

                        // Log details
                        Logger.Instance.LogPrimary($"Name: {name}");
                        Logger.Instance.LogPrimary($"Description: {description}");
                        Logger.Instance.LogPrimary($"Class Guid: {classGuid}");
                        Logger.Instance.LogPrimary($"Manufacturer: {manufacturer}");
                        Logger.Instance.LogPrimary($"Service: {service}");
                        Logger.Instance.LogPrimary($"Problem: {problem}");
                        Logger.Instance.LogPrimary($"Resolution: {resolution}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enumerating devices: {ex.Message}");
            }
        }

        private static string GetProblemDescription(int errorCode)
        {
            switch (errorCode)
            {
                case 1: return "This device is not configured correctly.";
                case 10: return "This device cannot start.";
                case 28: return "The drivers for this device are not installed.";
                case 31: return "This device is not working properly because Windows cannot load the drivers required for this device.";
                case 43: return "Windows has stopped this device because it has reported problems.";
                case 45: return "This device is currently not connected to the computer.";
                case 48: return "The software for this device has been blocked from starting because it is known to have problems with Windows.";
                case 49: return "Windows cannot start new hardware devices because the system hive is too large.";
                default: return $"Unknown error code: {errorCode}.";
            }
        }

        private static string GetProblemResolution(int errorCode)
        {
            switch (errorCode)
            {
                case 1: return "Update the driver. If that does not work, check the hardware documentation.";
                case 10: return "Ensure that the device is properly installed. Update or reinstall the driver.";
                case 28: return "Install the appropriate drivers for this device.";
                case 31: return "Try reinstalling the drivers for this device.";
                case 43: return "One of the drivers controlling the device notified the operating system that the device failed in some manner. Check the hardware documentation.";
                case 45: return "Reconnect the device to the computer.";
                case 48: return "Contact the device manufacturer for a driver update.";
                case 49: return "Reduce the number of devices installed on this computer.";
                default: return "Refer to the Windows documentation for more information.";
            }
        }
    }
}

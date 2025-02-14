using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning
{
    public class DisabledDevicesScan
    {
        public static void EnumerateDisabledDevices()
        {
            try
            {
                // Connect to the WMI namespace
                ManagementScope scope = new ManagementScope(@"\\.\root\cimv2");
                scope.Connect();

                // Query for disabled devices
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerUserConfig = TRUE");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    bool devicesFound = false;

                    foreach (ManagementObject device in searcher.Get())
                    {
                        devicesFound = true;

                        // Extract details
                        string name = device["Name"]?.ToString() ?? "Unknown";
                        string description = device["Description"]?.ToString() ?? "Unknown";
                        string classGuid = device["ClassGuid"]?.ToString() ?? "Unknown";
                        string manufacturer = device["Manufacturer"]?.ToString() ?? "Unknown";
                        string service = device["Service"]?.ToString() ?? "Unknown";

                        // Log details
                        Logger.Instance.LogPrimary($"Name: {name}");
                        Logger.Instance.LogPrimary($"Description: {description}");
                        Logger.Instance.LogPrimary($"Class Guid: {classGuid}");
                        Logger.Instance.LogPrimary($"Manufacturer: {manufacturer}");
                        Logger.Instance.LogPrimary($"Service: {service}");
                        Logger.Instance.LogPrimary($"Status: Disabled by User");
                    }

                    if (!devicesFound)
                    {
                        Logger.Instance.LogPrimary("No disabled devices found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enumerating disabled devices: {ex.Message}");
            }
        }
    }
}

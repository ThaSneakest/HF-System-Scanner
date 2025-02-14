using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner
{
    public class DeviceHandler
    {
        public void DEVICES(string FIX)
        {
            string LABEL1 = "Scanning"; // Example value for label
            string SCANB = "Scan";      // Example value for scanning label
            string DEVICE1 = "Device";  // Example device

            // Update GUI or log scanning information (Here we're just printing to the console for example)
            Console.WriteLine($"{SCANB} {DEVICE1}");

            // Query WMI for PnP devices
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_PnPEntity");

            try
            {
                ManagementObjectCollection devices = searcher.Get();

                foreach (ManagementObject device in devices)
                {
                    string NAME = device["Name"]?.ToString();
                    string DESCRIPTION = device["Description"]?.ToString();
                    string CLASSGUID = device["ClassGuid"]?.ToString();
                    string MANUFACTURER = device["Manufacturer"]?.ToString();
                    string SERVICE = device["Service"]?.ToString();
                    string CODE = "", CODEMSG = "";

                    int configManagerErrorCode = Convert.ToInt32(device["ConfigManagerErrorCode"]);

                    switch (configManagerErrorCode)
                    {
                        case 1:
                            CODE = ": This device is not configured correctly. (Code1)";
                            CODEMSG = "You may be prompted to provide the path of the driver. Windows may have the driver built-in, or may still have the driver files installed from the last time that you set up the device.";
                            break;
                        case 3:
                            CODE = ": The driver for this device might be corrupted. (Code3)";
                            CODEMSG = "If the driver is corrupted, uninstall the driver and scan for new hardware to install the driver again.";
                            break;
                        case 10:
                            CODE = ": This device cannot start. (Code10)";
                            CODEMSG = "Device failed to start. Click \"Update Driver\" to update the drivers for this device.";
                            break;
                        case 12:
                            CODE = ": This device cannot find enough free resources. (Code12)";
                            CODEMSG = "Two devices have been assigned the same input/output (I/O) ports.";
                            break;
                        // Add other cases based on error codes as needed
                        default:
                            break;
                    }

                    if (!string.IsNullOrEmpty(CODE))
                    {
                        // Log device information and error code message
                        //File.AppendAllText(Logger.WildlandsLogFile, $"Name: {NAME}\nDescription: {DESCRIPTION}\nClass Guid: {CLASSGUID}\nManufacturer: {MANUFACTURER}\nService: {SERVICE}\nProblem: {CODE}\nResolution: {CODEMSG}\n\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying WMI: {ex.Message}");
            }
        }
        public static List<string> ListFaultyDevices()
        {
            List<string> faultyDevices = new List<string>();

            // Query WMI for faulty devices in Device Manager
            string query = "SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode != 0";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection queryCollection = searcher.Get();

            foreach (ManagementObject device in queryCollection)
            {
                string deviceName = device["Name"]?.ToString();
                string errorCode = device["ConfigManagerErrorCode"]?.ToString();
                string deviceId = device["DeviceID"]?.ToString();

                faultyDevices.Add($"Device: {deviceName}, Error Code: {errorCode}, Device ID: {deviceId}");
            }

            return faultyDevices;
        }
    }
}

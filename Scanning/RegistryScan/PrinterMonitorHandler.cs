using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

//working and tested needs whitelist
public class PrinterMonitorHandler
{
    private static string Size = string.Empty;
    private static string Date = string.Empty;
    private static string Company = string.Empty;

    public static void PrintMonitors()
    {
        try
        {
            string keyPath = @"SYSTEM\CurrentControlSet\Control\Print\Monitors";

            // Open the registry key
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (registryKey == null)
                {
                    Console.WriteLine($"Registry key not found: HKLM\\{keyPath}");
                    return;
                }

                string[] subKeys = registryKey.GetSubKeyNames();
                List<string> arrayReg = new List<string>();

                foreach (var subKey in subKeys)
                {
                    string att = string.Empty;
                    string subKeyPath = keyPath + "\\" + subKey;

                    // Open the subkey
                    using (RegistryKey subRegistryKey = registryKey.OpenSubKey(subKey))
                    {
                        if (subRegistryKey == null)
                            continue;

                        // Read the "Driver" value from the subkey
                        string driverPath = subRegistryKey.GetValue("Driver") as string;

                        if (string.IsNullOrEmpty(driverPath))
                            continue;

                        // Process the driver path
                        ProcessDriver(driverPath);

                        string filePath = driverPath;

                        string val1;
                        // Check if the file exists
                        if (File.Exists(filePath))
                        {
                            val1 = $"{filePath} [{Size} {Date}] {Company}";

                            // Simulate reading checkbox status (example logic for skipping certain drivers)
                            bool checkboxRead = true; // Replace with actual control reading logic if needed
                            if (checkboxRead && Regex.IsMatch(val1, @"(?i):\\Windows\\System32\\(AppMon|localspl|FXSMON|tcpmon|usbmon|APMon|WSDMon|LPRMon)\.dll.+Microsoft Corporation\)$"))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            val1 = $"{driverPath} (File Not Found)";
                        }

                        // Log the processed value
                        Logger.Instance.LogPrimary($"HKLM\\...\\Print\\Monitors\\{subKey}: {val1}{att}");
                        arrayReg.Add(val1);
                    }
                }

                // Handle `arrayReg` as needed after processing
                Console.WriteLine("Processing completed for Print Monitors.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PrintMonitors: {ex.Message}");
        }
    }

    // Simulated method for processing the driver path
    private static void ProcessDriver(string driverPath)
    {
        // Simulate gathering details about the driver
        Size = "10KB"; // Example size
        Date = DateTime.Now.ToString("yyyy-MM-dd"); // Example date
        Company = "Example Company"; // Example company name
    }
  
}

using DevExpress.Utils.Drawing.Helpers;
using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;
using System.Management;
using System.Collections.Generic;


public static class ServiceHandler
{
    // Define whitelist and blacklist




    public static void ScanServices()
    {
        try
        {
            string wmiQuery = "SELECT * FROM Win32_Service";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);

            foreach (ManagementObject service in searcher.Get())
            {
                string serviceName = service["Name"]?.ToString();
                string displayName = service["DisplayName"]?.ToString();
                string serviceState = service["State"]?.ToString();
                string startMode = service["StartMode"]?.ToString();
                string executablePath = service["PathName"]?.ToString();
                string processId = service["ProcessId"]?.ToString();
                string company = FileUtils.GetFileDetails(executablePath, "CompanyName");
                string fileSize = FileUtils.GetFileSize(executablePath)?.ToString() ?? "Unknown";
                string fileVersion = FileUtils.GetFileDetails(executablePath, "FileVersion");
                string installDate = service["InstallDate"] != null ? ManagementDateTimeConverter.ToDateTime(service["InstallDate"].ToString()).ToShortDateString() : "Unknown";

                // Skip whitelisted services
                if (ServiceWhitelist.WhitelistedServices.Contains(serviceName))
                {
                    continue;
                }

                // Format the service state (R for Running, S for Stopped, U for Unknown)
                string stateCode;
                switch (serviceState)
                {
                    case "Running":
                        stateCode = "R";
                        break;
                    case "Stopped":
                        stateCode = "S";
                        break;
                    default:
                        stateCode = "U";
                        break;
                }

                // Format the start type (R2 for Auto, S2 for Manual, etc.)
                string startTypeCode;
                switch (startMode)
                {
                    case "Auto":
                        startTypeCode = "R2";
                        break;
                    case "Manual":
                        startTypeCode = "S2";
                        break;
                    default:
                        startTypeCode = "S4"; // Disabled or unknown
                        break;
                }

                // Check if the service is blacklisted
                bool isBlacklisted = BlacklistSrv.BlacklistedServices.Contains(serviceName);

                // Format log message
                string logMessage = $"{stateCode}{startTypeCode} {serviceName}; {executablePath} [{fileSize} {installDate}] ({company} -> {fileVersion})";

                if (isBlacklisted)
                {
                    logMessage += " <---- Possible Malicious Service";
                }

                // Log the service information
                Logger.Instance.LogPrimary(logMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning services: {ex.Message}");
        }
    }
}

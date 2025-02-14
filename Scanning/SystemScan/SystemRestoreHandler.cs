using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Wildlands_System_Scanner.Registry;

public class SystemRestoreHandler
{
    private const string HADDITION = "log.txt";
    private const string RP1 = "Restore Point 1";
    private const string RP2 = "Error reading restore points";
    private const string UPD1 = "Update";
    private const int OSNUM = 6; // Placeholder
    private const string ERR0 = "Error occurred"; // Placeholder
    private const string RP3 = "Restore Point Error"; // Placeholder
    private const string SCANB = "Scanning";

    private const int BEGIN_SYSTEM_CHANGE = 100;
    private const int END_SYSTEM_CHANGE = 101;
    private const int MODIFY_SETTINGS = 12;

    public void CreateRestorePoint()
    {
        // Calculate free space percentage
        double totalSpace = GetDriveSpaceTotal(@"C:\") / 1024.0;
        double freeSpace = GetDriveSpaceFree(@"C:\") / 1024.0;
        double percentage = Math.Round((freeSpace / totalSpace) * 100.0, 2);

        // Check for low disk space
        if (percentage < 10)
        {
            LogError(1, $"{percentage}% free space");
            return;
        }

        if (freeSpace < 10)
        {
            LogError(2, $"{freeSpace} GB free space");
            return;
        }

        // System restore configuration checks for older OS versions
        if (OSNUM < 6)
        {
            string key = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore";
            if (ReadIntValue(key, "DisableSR") == 1)
            {
                RegistryValueHandler.DeleteRegistryValue(key, "DisableSR");
            }
        }

        // Remove any policies that disable restore points
        string policyKey = @"HKLM\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore";
        if (RegistryKeyHandler.RegistryKeyExists(policyKey))
        {
            RegistryKeyHandler.DeleteRegistryKey(policyKey);
        }

        // Enable System Restore using WMI
        var restoreObj = new ManagementObject("winmgmts:{impersonationLevel=impersonate}!root/default:SystemRestore");
        if (restoreObj == null)
        {
            LogError(3, "Unable to access SystemRestore WMI object");
            return;
        }

        try
        {
            // Invoke the WMI method to enable system restore
            var result = restoreObj.InvokeMethod("Enable", new object[] { @"C:\" });
            if (result == null || (uint)result != 0)
            {
                LogError(4, "Failed to enable SystemRestore");
                return;
            }
        }
        catch (ManagementException ex)
        {
            LogError(4, "SystemRestore Enable method exception: " + ex.Message);
            return;
        }

        // Create a restore point using native APIs
        var restorePointInfo = new RESTOREPOINTINFO
        {
            dwEventType = BEGIN_SYSTEM_CHANGE,
            dwRestorePtType = MODIFY_SETTINGS,
            llSequenceNumber = 0,
            szDescription = "Restore Point Created by Code"
        };

        try
        {
            bool created = NativeMethods.SRSetRestorePointW(ref restorePointInfo);
            if (!created)
            {
                LogError(5, "Failed to create restore point");
                return;
            }

            // End the restore point
            restorePointInfo.dwEventType = END_SYSTEM_CHANGE;
            NativeMethods.SRSetRestorePointW(ref restorePointInfo);
        }
        catch (Exception ex)
        {
            LogError(6, "Exception creating restore point: " + ex.Message);
        }
    }

    private void LogError(int code, string message)
    {
        Console.WriteLine($"Error {code}: {message}");
        // Optionally write to a log file
        File.AppendAllText(HADDITION, $"Error {code}: {message}\n");
    }

    private double GetDriveSpaceTotal(string drive)
    {
        DriveInfo driveInfo = new DriveInfo(drive);
        return driveInfo.TotalSize / 1_073_741_824.0; // Convert bytes to GB
    }

    private double GetDriveSpaceFree(string drive)
    {
        DriveInfo driveInfo = new DriveInfo(drive);
        return driveInfo.AvailableFreeSpace / 1_073_741_824.0; // Convert bytes to GB
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RESTOREPOINTINFO
    {
        public int dwEventType;
        public int dwRestorePtType;
        public long llSequenceNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szDescription;
    }

    private static class NativeMethods
    {
        [DllImport("srclient.dll", CharSet = CharSet.Unicode)]
        public static extern bool SRSetRestorePointW(ref RESTOREPOINTINFO pRestorePtSpec);
    }

    public static int ReadIntValue(string key, string valueName)
    {
        using (var regKey = Registry.LocalMachine.OpenSubKey(key))
        {
            return regKey != null
                ? Convert.ToInt32(regKey.GetValue(valueName, 0))
                : 0;
        }
    }
    public static List<string> ListSystemRestorePoints()
    {
        List<string> restorePoints = new List<string>();

        // Query WMI for system restore points
        ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\default");
        ObjectQuery query = new ObjectQuery("SELECT * FROM SystemRestore");
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
        ManagementObjectCollection queryCollection = searcher.Get();

        foreach (ManagementObject m in queryCollection)
        {
            string description = m["Description"]?.ToString();
            string creationTime = ManagementDateTimeConverter.ToDateTime(m["CreationTime"].ToString()).ToString();
            string restorePointType = m["RestorePointType"]?.ToString();
            string sequenceNumber = m["SequenceNumber"]?.ToString();

            restorePoints.Add($"Description: {description}, Creation Time: {creationTime}, Type: {restorePointType}, Sequence Number: {sequenceNumber}");
        }

        return restorePoints;
    }
    public static void EnumerateSystemRestorePoints()
    {
        try
        {
            Console.WriteLine("=== Enumerating System Restore Points ===");

            // Connect to the WMI namespace
            ManagementScope scope = new ManagementScope(@"\\.\root\default");
            scope.Connect();

            // Query for system restore points
            ObjectQuery query = new ObjectQuery("SELECT * FROM SystemRestore");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                var restorePoints = searcher.Get();
                if (restorePoints.Count == 0)
                {
                    Console.WriteLine("No System Restore Points found.");
                    Logger.Instance.LogPrimary("No System Restore Points found.");
                    return;
                }

                foreach (ManagementObject restorePoint in restorePoints)
                {
                    // Extract details
                    int sequenceNumber = Convert.ToInt32(restorePoint["SequenceNumber"]);
                    string description = restorePoint["Description"]?.ToString() ?? "No Description";
                    DateTime creationTime = ManagementDateTimeConverter.ToDateTime(restorePoint["CreationTime"].ToString());
                    int restorePointType = Convert.ToInt32(restorePoint["RestorePointType"]);
                    int eventType = Convert.ToInt32(restorePoint["EventType"]);

                    // Format the output
                    string logEntry = $"Restore Point: {sequenceNumber} - {description}\n" +
                                      $"  Created On: {creationTime:yyyy-MM-dd HH:mm:ss}\n" +
                                      $"  Type: {GetRestorePointType(restorePointType)}\n" +
                                      $"  Event Type: {GetEventType(eventType)}";

                    // Output and log the result
                    Console.WriteLine(logEntry);
                    Logger.Instance.LogPrimary(logEntry);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while enumerating restore points: {ex.Message}");
            Logger.Instance.LogPrimary($"Error: {ex.Message}");
        }
    }

    private static string GetRestorePointType(int type)
    {
        switch (type)
        {
            case 0: return "Application Install";
            case 1: return "Application Uninstall";
            case 10: return "System Restore";
            case 12: return "Device Driver Install";
            case 13: return "Software Distribution";
            case 14: return "Manual Restore Point";
            case 15: return "Windows Backup";
            default: return "Unknown Type";
        }
    }

    private static string GetEventType(int type)
    {
        switch (type)
        {
            case 101: return "Begin System Change";
            case 102: return "End System Change";
            case 103: return "Begin Nested System Change";
            case 104: return "End Nested System Change";
            default: return "Unknown Event";
        }
    }
}

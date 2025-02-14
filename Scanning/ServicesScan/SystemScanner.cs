using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static class SystemScanner
{
    public static void ScanServices(string logFilePath, string tempDir, string systemPath, string softwarePath, string scanLabel, string recoveryMode, bool isRecoveryMode, Dictionary<string, string> registryKeys)
    {
        string serviceKey = $@"HKLM\{systemPath}\Services";
        List<string> lockedServices = new List<string>();
        List<string> validServices = new List<string>();
        List<string> invalidServices = new List<string>();

        // Start scanning services
        UpdateLabel(scanLabel, "Scanning Services...");

        if (isRecoveryMode)
        {
            HandleRecoveryMode(serviceKey, lockedServices, registryKeys);
        }
        else
        {
            HandleNonRecoveryMode(serviceKey, lockedServices, registryKeys, tempDir, logFilePath);
        }

        // Save scanned data
        SaveScannedServices(logFilePath, tempDir, lockedServices, validServices, invalidServices);
    }

    private static void HandleRecoveryMode(string serviceKey, List<string> lockedServices, Dictionary<string, string> registryKeys)
    {
        // Implement recovery mode-specific logic here
        foreach (var key in registryKeys.Keys)
        {
            string value = registryKeys[key];
            lockedServices.Add($"{key}: {value} <==== Locked in Recovery Mode");
        }
    }

    private static void HandleNonRecoveryMode(string serviceKey, List<string> lockedServices, Dictionary<string, string> registryKeys, string tempDir, string logFilePath)
    {
        // Implement non-recovery mode-specific logic here
        foreach (var key in registryKeys.Keys)
        {
            string value = registryKeys[key];
            if (Regex.IsMatch(value, @"InvalidPattern"))
            {
                lockedServices.Add($"{key}: {value} <==== Needs Unlocking");
            }
            else
            {
                lockedServices.Add($"{key}: {value} <==== Valid");
            }
        }

        // Additional processing can be added as needed
    }

    private static void SaveScannedServices(string logFilePath, string tempDir, List<string> lockedServices, List<string> validServices, List<string> invalidServices)
    {
        string serviceLogPath = Path.Combine(tempDir, "services.log");

        // Save locked services
        File.WriteAllLines(serviceLogPath, lockedServices);

        // Save valid services
        if (validServices.Count > 0)
        {
            File.AppendAllLines(serviceLogPath, validServices);
        }

        // Save invalid services
        if (invalidServices.Count > 0)
        {
            File.AppendAllLines(serviceLogPath, invalidServices);
        }

        // Append to main log file
        File.AppendAllText(logFilePath, File.ReadAllText(serviceLogPath));
        File.Delete(serviceLogPath);
    }

    private static void UpdateLabel(string label, string message)
    {
        Console.WriteLine($"{label}: {message}");
    }

    public static string ProcessPath(string path, string fallbackPath)
    {
        if (CreateFile(path))
        {
            return $"{path} <==== {UpdateIndicator} ({NoAccess})";
        }
        else
        {
            return $"{fallbackPath} [X]";
        }
    }

    // Mock implementation of CreateFile for demonstration purposes.
    // Replace this with the actual logic as needed.
    private static bool CreateFile(string path)
    {
        try
        {
            // Simulate creating or accessing the file
            return File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    // Constants for UpdateIndicator and NoAccess
    private const string UpdateIndicator = "UpdateIndicator";
    private const string NoAccess = "NoAccess";

}

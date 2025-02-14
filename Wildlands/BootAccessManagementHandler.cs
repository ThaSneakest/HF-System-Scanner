using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Utilities;

public class BootAccessManagementHandler
{
    private static readonly string BAMLOG = @"C:\Wildlands\BAMLOG.txt";

    public static void Run()
    {
        // Exit early if the OS version is unsupported
        if (SystemConstants.OperatingSystemNumberMajor < 6)
        {
            Logger.LogWarning("Unsupported OS version. Exiting...");
            return;
        }

        try
        {
            if (SystemConstants.BootMode == "Recovery")
            {
                HandleRecoveryMode();
            }

            ProcessFile(Path.Combine(Environment.GetEnvironmentVariable("SYSTEMROOT"), "winsrv.dll"));

            // Perform BCD operations
            ProcessBCD();
        }
        catch (Exception ex)
        {
            Logger.LogError("An error occurred in Run method", ex);
        }
    }

    private static void HandleRecoveryMode()
    {
        string systemRoot = Environment.GetEnvironmentVariable("SYSTEMROOT");

        if (string.IsNullOrEmpty(systemRoot))
        {
            Logger.LogError("SYSTEMROOT environment variable is not set.");
            return;
        }

        if (SystemConstants.OperatingSystemNumberMajor < 10)
        {
            ProcessFile(Path.Combine(systemRoot, "codeintegrity", "Bootcat.cache"));
        }

        ProcessFile(Path.Combine(systemRoot, "catroot", "{F750E6C3-38EE-11D1-85E5-00C04FC295EE}"));

        if (SystemConstants.OperatingSystemNumberMajor == 10)
        {
            ProcessFile(Path.Combine(systemRoot, "InputHost.dll"));
        }
    }

    private static void ProcessFile(string filePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                BAMMIS(filePath);
            }
            else
            {
                Logger.LogWarning($"File not found: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error processing file: {filePath}", ex);
        }
    }

    public static void BAMMIS(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                string logMessage = $"{filePath} MISS <==== Update Indicator{Environment.NewLine}";
                Logger.FileWrite("FRST.txt", logMessage);
                Logger.LogInfo($"Logged missing file: {filePath}");
            }
            else
            {
                Logger.LogInfo($"File exists: {filePath}");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogError($"Access denied for file: {filePath}", ex);
        }
        catch (IOException ex)
        {
            Logger.LogError($"I/O error for file: {filePath}", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unexpected error for file: {filePath}", ex);
        }
    }

    public static void ProcessBCD()
    {
        try
        {
            // List all entries in the BCD store
            string bcdEntries = CommandHandler.RunCommandString(FolderConstants.System32 + @"\bcdedit.exe /enum");
            Logger.LogInfo("BCD Entries:\n" + bcdEntries);

            // Check and update "testsigning" status
            if (Regex.IsMatch(bcdEntries, @"(?i)testsigning\s*Yes"))
            {
                Logger.FileWrite(BAMLOG, $"\r\n\r\ntestsigning: ==> {StringConstants.TESTS} <==== {StringConstants.UPD1}\r\n");
            }

            // Enable recovery mode if disabled
            EnableRecoveryMode();

            // Check and log safeboot settings
            if (Regex.IsMatch(bcdEntries, @"(?i)safeboot\s"))
            {
                string safebootMode = Regex.Replace(bcdEntries, @"(?is).*safeboot\s+(\w+?)\r?\n.*", "$1");
                Logger.FileWrite(BAMLOG, $"\r\n\r\nsafeboot: {safebootMode} => {StringConstants.BCDSM} <==== {StringConstants.UPD1}\r\n");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error processing BCD", ex);
        }
    }

    private static void EnableRecoveryMode()
    {
        try
        {
            string read = CommandHandler.RunCommandString(FolderConstants.System32 + @"\bcdedit.exe /enum {default}");

            if (Regex.IsMatch(read, @"(?i)recoveryenabled\s*No"))
            {
                string command = FolderConstants.System32 + @"\bcdedit.exe /set {default} recoveryenabled yes";
                CommandHandler.RunWait(command);

                // Verify the change
                read = CommandHandler.RunCommandString(FolderConstants.System32 + @"\bcdedit.exe /enum {default}");
                if (Regex.IsMatch(read, @"(?i)recoveryenabled\s*Yes"))
                {
                    Logger.FileWrite(BAMLOG, $"\r\nBCD (recoveryenabled=No -> recoveryenabled=Yes) <==== {StringConstants.RESTORED}\r\n");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error enabling recovery mode", ex);
        }
    }
}

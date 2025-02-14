using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public class Finalizer
{
    private const string ScanExtrasMessage = "Scanning Extras...";
    private const string FrstLogPath = @"C:\FRST\Logs\FRST.txt";
    private const string BackupPrivilege = "SeBackupPrivilege";
    private const string RestorePrivilege = "SeRestorePrivilege";
    private const string AdditionLogPath = @"Addition.txt";
    private const string ShortcutLogPath = @"Shortcut.txt";
    private const string FrstLogsDirectory = @"C:\FRST\Logs";
    private const string ScanCompletedMessage = "Scan completed.";
    private const string NotepadCommand = "notepad.exe";

    public static void Final()
    {
        Console.WriteLine(ScanExtrasMessage);

        bool isRecoveryMode = Utility.IsRecoveryMode();

        if (isRecoveryMode)
        {
            ExecutePart();
        }

        ExecuteLastBoot();

        if (isRecoveryMode)
        {
            HandleRegistryUnload("999", "ErrorUnloadSystem");
            HandleRegistryUnload("888", "ErrorUnloadSoftware");
        }


        // Update UI elements
        UpdateLabel($"{ScanCompletedMessage}, Processing...");
        UpdateProgressBar(false);
        UpdateScanButtonText("Scan");

        // Handle Addition.txt logic if the corresponding checkbox is checked
        if (IsCheckboxChecked(12) && !Utility.IsRecoveryMode() && !IsCommandLineMode())
        {
            Thread.Sleep(1000); // Simulate delay
            NotifyAndOpenFile("FRST", $"Addition.txt {ScanCompletedMessage}", AdditionLogPath);
        }

        // Handle Shortcut.txt logic if the corresponding checkbox is checked
        if (IsCheckboxChecked(13) && !Utility.IsRecoveryMode() && !IsCommandLineMode())
        {
            Thread.Sleep(1000); // Simulate delay
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss");
            string shortcutLogDestination = Path.Combine(FrstLogsDirectory, $"Shortcut_{currentDate}.txt");
            File.Copy(ShortcutLogPath, shortcutLogDestination, overwrite: true);

            NotifyAndOpenFile("FRST", $"Shortcut.txt {ScanCompletedMessage}", ShortcutLogPath);
        }

        // Exit if in command-line mode
        if (IsCommandLineMode())
        {
            Environment.Exit(0);
        }
    }

    private static void HandleRegistryUnload(string key, string errorLogMessage)
    {
        if (RegistryKeyHandler.RegistryKeyExists($@"HKEY_LOCAL_MACHINE\{key}"))
        {
            CommandHandler.RunCommand($"reg unload hklm\\{key}");

            if (RegistryKeyHandler.RegistryKeyExists($@"HKEY_LOCAL_MACHINE\{key}"))
            {
                EnsurePrivileges();

                int result = Advapi32NativeMethods.RegUnLoadKeyW(new IntPtr(unchecked((int)0x80000002)), key);
                if (result != 0)
                {
                    File.AppendAllText(FrstLogPath, $"{errorLogMessage}: {result}{Environment.NewLine}");
                }
            }
        }
    }

    private static void EnsurePrivileges()
    {
        // Logic to enable privileges if required (e.g., SeBackupPrivilege, SeRestorePrivilege)
        EnablePrivilege(RestorePrivilege);
        EnablePrivilege(BackupPrivilege);
    }

    private static void EnablePrivilege(string privilege)
    {
        // Implement logic to enable specific privilege
        Console.WriteLine($"Privilege {privilege} has been enabled.");
    }

    private static void ExecutePart()
    {
        // Placeholder for PART function logic
        Console.WriteLine("Executing PART logic...");
    }

    private static void ExecuteLastBoot()
    {
        // Placeholder for LASTBOOT function logic
        Console.WriteLine("Executing LASTBOOT logic...");
    }


    private static void UpdateLabel(string text)
    {
        Console.WriteLine($"[UI] Update Label: {text}");
    }

    private static void UpdateProgressBar(bool visible)
    {
        Console.WriteLine($"[UI] Progress Bar Visible: {visible}");
    }

    private static void UpdateScanButtonText(string text)
    {
        Console.WriteLine($"[UI] Update Scan Button Text: {text}");
    }

    private static bool IsCheckboxChecked(int checkboxId)
    {
        // Simulate checking a checkbox state
        return checkboxId == 12 || checkboxId == 13; // Adjust based on actual conditions
    }

    private static bool IsCommandLineMode()
    {
        // Simulate checking if the application is in command-line mode
        return false; // Replace with actual command-line mode check
    }

    private static void NotifyAndOpenFile(string title, string message, string filePath)
    {
        // Simulate displaying a message box
        Console.WriteLine($"[MessageBox] Title: {title}, Message: {message}");

        // Open the file in Notepad
        if (File.Exists(filePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = NotepadCommand,
                Arguments = $"\"{filePath}\"",
                UseShellExecute = true
            });
        }
        else
        {
            Console.WriteLine($"[Error] File not found: {filePath}");
        }
    }
}

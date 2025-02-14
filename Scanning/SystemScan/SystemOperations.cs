using System;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

public static class SystemOperations
{
    public static void PerformSystemOperations()
    {
        string tempDir = Path.GetTempPath();
        string setL1Path = Path.Combine(tempDir, "SetL1.txt");
        string setL2Path = Path.Combine(tempDir, "SetL2.txt");

        // Cleanup temporary files if they exist
        if (File.Exists(setL1Path)) File.Delete(setL1Path);
        if (File.Exists(setL2Path)) File.Delete(setL2Path);

        // Get all drives
        DriveInfo[] drives = DriveInfo.GetDrives();
        foreach (var drive in drives)
        {
            if (drive.DriveType == DriveType.Fixed &&
                !drive.Name.Contains("X") &&
                File.Exists(Path.Combine(drive.Name, @"windows\system32\config\software")))
            {
                string osLoad = null;
                try
                {
                    // Load the SOFTWARE hive
                    string hivePath = Path.Combine(drive.Name, @"Windows\System32\config\software");
                    Registry.LocalMachine.CreateSubKey("888");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c reg load HKLM\\888 \"{hivePath}\"",
                        WindowStyle = ProcessWindowStyle.Hidden
                    })?.WaitForExit();

                    osLoad = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\888\Microsoft\Windows NT\CurrentVersion", "ProductName", null);

                    // Unload the hive
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c reg unload HKLM\\888",
                        WindowStyle = ProcessWindowStyle.Hidden
                    })?.WaitForExit();

                    if (osLoad != null)
                    {
                        Console.WriteLine($"Found OS: {osLoad} on drive {drive.Name}");
                        var userChoice = PromptUser($"Detected OS: {osLoad}\nDrive: {drive.Name}\nDo you want to switch the system drive?");
                        if (userChoice)
                        {
                            if (drive.Name.Equals("C:\\", StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                            else
                            {
                                // Write commands to reassign drive letters
                                File.WriteAllText(setL1Path, "Select volume=C:\nassign letter=Y\nexit");
                                File.WriteAllText(setL2Path, $"Select volume={drive.Name.TrimEnd('\\')}\nassign letter=C\nexit");

                                Console.WriteLine("Reassigning drive letters...");
                                ExecuteDiskPart(setL1Path);
                                ExecuteDiskPart(setL2Path);

                                // Cleanup temporary files
                                File.Delete(setL1Path);
                                File.Delete(setL2Path);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing drive {drive.Name}: {ex.Message}");
                }
            }
        }

        // Additional operations for restoring privileges and testing
        RestorePrivileges("SeRestorePrivilege", true);
        RestorePrivileges("SeBackupPrivilege", true);

        string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_test");
        File.WriteAllText(testFilePath, "test");
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }
        else
        {
            foreach (var drive in drives)
            {
                string frstPath = Path.Combine(drive.Name, "frst.exe");
                if (File.Exists(frstPath))
                {
                    Console.WriteLine("Found FRST executable. Launching...");
                    Process.Start(frstPath);
                    return;
                }
            }
        }

        Console.WriteLine("Operation complete.");
    }

    private static void ExecuteDiskPart(string scriptPath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c diskpart /s \"{scriptPath}\"",
            WindowStyle = ProcessWindowStyle.Hidden
        })?.WaitForExit();
    }

    private static void RestorePrivileges(string privilegeName, bool enable)
    {
        // Simulated function to handle privilege restoration
        Console.WriteLine($"Privilege '{privilegeName}' restored: {enable}");
    }

    private static bool PromptUser(string message)
    {
        // Simulate a user prompt (e.g., using a MessageBox in GUI applications)
        Console.WriteLine(message);
        Console.WriteLine("Press Y to confirm, N to cancel:");
        ConsoleKey key = Console.ReadKey(true).Key;
        return key == ConsoleKey.Y;
    }
}

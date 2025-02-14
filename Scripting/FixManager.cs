using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;

public class FixManager
{
    private static string FixLogFile = @"C:\Path\to\Fixlog.txt";  // Adjust log file path
    private static string ScriptDir = @"C:\Path\to\script";  // Adjust directory path
    private static string C = @"C:\Path\to\files"; // Adjust directory path
    private static string BootMode = "Normal"; // Adjust Boot Mode
    private static string EndMessage = "Fix Complete";
    private static string HostFilePath = @"\Windows\System32\drivers\etc\hosts";

    private static void BBBBRBL()
    {
        // Simulate GUI updates here
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        using (StreamWriter fixLog = new StreamWriter(FixLogFile, append: true))
        {
            fixLog.WriteLine($"{EndMessage} ({BootMode}): {currentDate}");

            if (File.Exists(Path.Combine(C, @"FRST\re")))
            {
                fixLog.WriteLine("==> " + "UPDATE1" + ": FIX4");
            }

            int i = 1;
            while (true)
            {
                string file = ReadFileLine(C + @"\frst\files", i);
                if (string.IsNullOrEmpty(file)) break;

                string path = @"\\?\" + file;
                if (!File.Exists(path))
                {
                    fixLog.WriteLine($"{file} => FIX5");
                    if (file.Contains(HostFilePath)) HostSFix1(file);
                }
                else
                {
                    FileAttributes attrib = File.GetAttributes(path);
                    if (attrib.HasFlag(FileAttributes.Directory))
                    {
                        string destination = GetDestination(file);
                        GrantPermissions(path, 1, 0);
                        bool dirDone = MoveDirectory(path, destination);
                        if (dirDone && !File.Exists(path))
                        {
                            Moved(file);
                        }
                        else
                        {
                            GrantPermissions(file, 1, 0);
                            bool dirDone1 = MoveDirectory(file, destination);
                            if (dirDone1 && !File.Exists(path))
                            {
                                Moved(file);
                            }
                            else
                            {
                                NotMoved(file);
                            }
                        }
                    }
                    else
                    {
                        string destination = GetDestination(file, 1);
                        bool dirDone = MoveFile(path, destination);
                        if (dirDone)
                        {
                            Moved(file);
                            if (file.Contains(HostFilePath)) HostSFix1(file);
                        }
                        else
                        {
                            GrantPermissions(path, 1, 0);
                            bool dirDone1 = MoveFile(path, destination);
                            if (dirDone1)
                            {
                                Moved(file);
                                if (file.Contains(HostFilePath)) HostSFix1(file);
                            }
                            else
                            {
                                NotMoved(file);
                                if (file.Contains(HostFilePath))
                                    fixLog.WriteLine($"Restore Hosts: {file}");
                            }
                        }
                    }
                }

                i++;
            }

            // Additional logic for "filesRem" and "keysRem"
            ProcessFilesRem(fixLog);
            ProcessKeysRem(fixLog);

            // Final log entry
            fixLog.WriteLine($"==== {EndMessage} Fixlog completed at {currentDate} ====");
        }

        // Clean up and move logs
        string cdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        File.Copy(FixLogFile, Path.Combine(C, @"FRST\Logs\Fixlog_" + cdDate + ".txt"));
        OpenFixLog();
    }


    public static void ProcessFilesRem(StreamWriter fixLog)
    {
        int ii = 1;
        while (true)
        {
            // Replace ReadFileLine with actual method for reading lines from the file
            string file = ReadFileLine(C + @"\frst\filesRem", ii);
            if (string.IsNullOrEmpty(file)) break;

            string path = @"\\?\" + file;
            path = Regex.Replace(path, @"\\(?!\?\\)", @"\");

            if (!File.Exists(path))
            {
                Deleted(file);
            }
            else
            {
                // Check if file is a directory by examining attributes
                if (GetFileAttributes(path).Contains("Directory"))
                {
                    GrantPermissions(file, 1, 1);
                    bool dirDone = RemoveDirectory(path);
                    if (dirDone && !File.Exists(path))
                    {
                        Deleted(file);
                    }
                    else
                    {
                        NotMoved(file);
                    }
                }
                else
                {
                    GrantPermissions(path, 1, 0);
                    if (Regex.IsMatch(GetFileAttributes(path), @"(?i)S|R|H"))
                    {
                        SetFileAttributes(path, "-SRH");
                    }

                    bool dirDone = DeleteFile(path);
                    if (dirDone)
                    {
                        Deleted(file);
                    }
                    else
                    {
                        NotMoved(file);
                    }
                }
            }

            ii++;
        }
    }

    private static void ProcessKeysRem(StreamWriter fixLog)
    {
        string keysRemPath = Path.Combine(C, @"\frst\keysRem"); // Using Path.Combine for better path handling

        if (File.Exists(keysRemPath))
        {
            if (keysRemPath.Contains("HKU\\"))
            {
                Load();
            }

            int k = 1;
            while (true)
            {
                string key = ReadFileLine(keysRemPath, k);
                if (string.IsNullOrEmpty(key)) break;

                DeleteRegistryKey(key, 1);
                k++;
            }

            if (LoadKeys().Length > 1) Unload();
            File.Delete(keysRemPath);
        }
    }


    private static string ReadFileLine(string filePath, int lineNumber)
    {
        string[] lines = File.ReadAllLines(filePath);
        return lines.Length >= lineNumber ? lines[lineNumber - 1] : string.Empty;
    }

    private static void OpenFixLog()
    {
        // Logic to open the log file in Notepad
        Process.Start("notepad", FixLogFile);
    }

    private static void Moved(string file)
    {
        // Logic for moved files
        Console.WriteLine($"Moved: {file}");
    }

    private static void NotMoved(string file)
    {
        // Logic for not moved files
        Console.WriteLine($"Not moved: {file}");
    }

    private static void Deleted(string file)
    {
        // Logic for deleted files
        Console.WriteLine($"Deleted: {file}");
    }

    private static void Deleted(string file, string reason)
    {
        // Logic for deleted files with reason
        Console.WriteLine($"Deleted: {file} due to {reason}");
    }

    private static void GrantPermissions(string path, int permission, int recursion)
    {
        // Implement logic to grant permissions on file/directory
        Console.WriteLine($"Granted permissions on {path}");
    }

    private static string GetFileAttributes(string path)
    {
        // Implement logic to get file attributes
        return File.GetAttributes(path).ToString();
    }

    private static bool MoveDirectory(string source, string destination)
    {
        try
        {
            Directory.Move(source, destination);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool MoveFile(string source, string destination)
    {
        try
        {
            File.Move(source, destination);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool RemoveDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool DeleteFile(string path)
    {
        try
        {
            File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetDestination(string file, int version = 0)
    {
        // Get the destination directory for the file
        return Path.Combine("C:\\Destination", file);
    }

    private static void HostSFix1(string file)
    {
        // Implement logic for fixing host file issues
        Console.WriteLine($"Fixed hosts file: {file}");
    }

    private static void Load()
    {
        // Logic to load keys
        Console.WriteLine("Loaded registry keys.");
    }

    private static void Unload()
    {
        // Logic to unload keys
        Console.WriteLine("Unloaded registry keys.");
    }

    private static void DeleteRegistryKey(string key, int version)
    {
        // Logic to delete registry key
        Console.WriteLine($"Deleted registry key: {key}");
    }

    private static string[] LoadKeys()
    {
        string registryPath = @"HKEY_LOCAL_MACHINE\Software\MySoftware"; // Update this to the correct registry path
        var keys = new List<string>();

        try
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        keys.Add(valueName); // Add the registry key values
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading registry: {ex.Message}");
        }

        return keys.ToArray(); // Return as an array of strings
    }

    private static void SetFileAttributes(string path, string attribute)
    {
        FileAttributes currentAttributes = File.GetAttributes(path);

        // Check if the attribute to be removed is "SRH"
        if (attribute == "-SRH")
        {
            currentAttributes &= ~FileAttributes.System;  // Remove the "System" attribute
            currentAttributes &= ~FileAttributes.Hidden;  // Remove the "Hidden" attribute
        }

        File.SetAttributes(path, currentAttributes);
    }

    private const string HFIXLOG = @"C:\path\to\log.txt"; // Adjust the log file path as needed
    private const string UPD1 = "Updated"; // Placeholder for the value of UPD1

    public void TimeA(IntPtr hwnd, int imsg, int iIdTimer, int iTime)
    {
        // Log the termination of the fixing process
        LogTermination();

        // Exit the process
        Environment.Exit(0);
    }

    private void LogTermination()
    {
        try
        {
            // Log message for termination due to the time limit
            string logMessage = $"{Environment.NewLine}Fixing is terminated due to reaching maximum fixing time of 60 minutes. <==== {UPD1}{Environment.NewLine}";

            // Write the log message to the file
            File.AppendAllText(HFIXLOG, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }

}

using DevExpress.Utils.Drawing.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Scripting;

//BootExecute tested and working
public static class SymlinkHandler
{


    public static void ProcessSymlink(string fixPath, string additionFilePath, string fixLogFilePath)
    {
        using (StreamWriter additionWriter = new StreamWriter(additionFilePath, true))
        using (StreamWriter logWriter = new StreamWriter(fixLogFilePath, true))
        {
            additionWriter.WriteLine($"\n==================== \"{fixPath}\" =============");

            string folder = DirectoryFix.ExtractFolderFromFix(fixPath);
            if (!Directory.Exists(folder))
            {
                logWriter.WriteLine("Not Found.");
            }
            else
            {
                var reparsePoints = GetReparsePoints(folder);
                if (reparsePoints.Length > 0)
                {
                    foreach (var entry in reparsePoints)
                    {
                        logWriter.WriteLine(entry);
                    }
                }
                logWriter.WriteLine("\n====== End of Symlink: ======\n");
            }
        }
    }


    private static string[] GetReparsePoints(string folderPath)
    {
        if (!folderPath.EndsWith("\\"))
        {
            folderPath += "\\";
        }

        var folderQueue = new Queue<string>();
        folderQueue.Enqueue(folderPath);

        var reparseList = new List<string>();

        while (folderQueue.Count > 0)
        {
            string currentPath = folderQueue.Dequeue();

            foreach (string entry in Directory.EnumerateFileSystemEntries(currentPath))
            {
                if (FileUtils.IsReparsePoint(entry))
                {
                    string target = GetReparsePointTarget(entry);
                    string timestamp = File.GetCreationTime(entry).ToString("yyyy-MM-dd HH:mm:ss");
                    reparseList.Add($"[{timestamp}] {entry} -> {target}");
                }
                else if (Directory.Exists(entry))
                {
                    folderQueue.Enqueue(entry);
                }
            }
        }

        return reparseList.ToArray();
    }


    public static void LogSmartScreenSettings(string key, string valueName, string additionFilePath)
    {
        using (StreamWriter additionWriter = new StreamWriter(additionFilePath, true))
        {
            object smartScreenValue = Registry.GetValue(key, valueName, null);
            if (smartScreenValue != null && smartScreenValue.ToString() == "0")
            {
                additionWriter.WriteLine($"{key} => ({valueName}: {smartScreenValue})");
            }
        }
    }

    public static void LogBootExecute()
    {
        try
        {
            // Log BootExecute information
            string bootExecuteKey = $@"System\CurrentControlSet\Control\Session Manager";
            object bootExecuteValue = Registry.GetValue($@"HKEY_LOCAL_MACHINE\{bootExecuteKey}", "BootExecute", null);

            if (bootExecuteValue != null)
            {
                if (bootExecuteValue is string[] bootExecuteArray)
                {
                    string bootExecute = string.Join(" ", bootExecuteArray)
                                          .Replace("\n", "")
                                          .Replace("\v", "")
                                          .Replace("*", "* ");
                    Logger.Instance.LogPrimary($"BootExecute: {bootExecute}");
                }
                else if (bootExecuteValue is string bootExecuteString)
                {
                    string bootExecute = bootExecuteString
                                         .Replace("\n", "")
                                         .Replace("\v", "")
                                         .Replace("*", "* ");
                    Logger.Instance.LogPrimary($"BootExecute: {bootExecute}");
                }
                else
                {
                    Logger.Instance.LogPrimary($"BootExecute: Unhandled data type ({bootExecuteValue.GetType()})");
                }
            }
            else
            {
                Logger.Instance.LogPrimary($"BootExecute key not found in: HKEY_LOCAL_MACHINE\\{bootExecuteKey}");
            }

            // Log AlternateShell information
            string alternateShellKey = $@"System\CurrentControlSet\Control\Session Manager";
            object alternateShellValue = Registry.GetValue($@"HKEY_LOCAL_MACHINE\{alternateShellKey}", "AlternateShell", null);

            if (alternateShellValue != null)
            {
                string alternateShell = alternateShellValue.ToString();
                if (!string.Equals(alternateShell, "cmd.exe", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Instance.LogPrimary($"AlternateShell: {alternateShell} <==== Update Required");
                }
            }
            else
            {
                Logger.Instance.LogPrimary($"AlternateShell key not found in: HKEY_LOCAL_MACHINE\\{alternateShellKey}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LogBootExecute: {ex.Message}");
        }
    }



    public static string GetReparsePointTarget(string path)
    {
        IntPtr hFile = Kernel32NativeMethods.CreateFile(
            path,
            NativeMethodConstants.GENERIC_READ,
            0,
            IntPtr.Zero,
            NativeMethodConstants.OPEN_EXISTING,
            NativeMethodConstants.FILE_FLAG_BACKUP_SEMANTICS | NativeMethodConstants.FILE_FLAG_OPEN_REPARSE_POINT,
            IntPtr.Zero);

        if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
        {
            throw new IOException($"Unable to open file handle for {path}", Marshal.GetLastWin32Error());
        }

        try
        {
            char[] targetBuffer = new char[1024];
            uint result = Kernel32NativeMethods.GetFinalPathNameByHandle(hFile, targetBuffer, (uint)targetBuffer.Length, 0);
            if (result == 0 || result >= targetBuffer.Length)
            {
                throw new IOException($"Unable to resolve reparse point target for {path}", Marshal.GetLastWin32Error());
            }

            string target = new string(targetBuffer, 0, (int)result);
            return target.StartsWith(@"\\?\") ? target.Substring(4) : target;
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(hFile);
        }
    }
}

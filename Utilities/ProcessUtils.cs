using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Blacklist;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.Whitelist;


public static class ProcessUtils
{
    private static List<string> ProcessList = new List<string>();

    public static string GetFormattedProcessDetails(int processId)
    {
        try
        {
            string processName = GetProcessName(processId);
            string processPath = GetProcessPathWMI(processId);
            string publisher = GetProcessPublisher(processPath);

            return $"({processName} ->) ({publisher}) {processPath} <{processId}>";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error formatting details for ProcessID={processId}: {ex.Message}");
            return $"Error retrieving details for ProcessID={processId}";
        }
    }

    public static void DisplayAllProcesses()
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                string processPath = GetProcessPathWMI(process.Id);
                if (string.IsNullOrEmpty(processPath))
                    continue;

                // Normalize the process path
                processPath = processPath.ToLowerInvariant();

                // Skip whitelisted processes
                if (IsWhitelisted(processPath))
                {
                    continue; // Do not log or display whitelisted processes
                }

                // Check if the process is blacklisted
                if (ProcessBlacklist.Blacklist.Any(blacklisted => processPath.Equals(blacklisted.ToLowerInvariant())))
                {
                    Logger.Instance.LogPrimary($"{processPath} <---- Malicious Process Found");
                    Console.WriteLine($"Malicious Process Found: {processPath}");
                    continue;
                }

                // Default process logging
                string processDetails = GetFormattedProcessDetails(process.Id);
                Logger.Instance.LogPrimary(processDetails);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied for ProcessID={process.Id}");
            }
            catch (SystemException ex)
            {
                Console.WriteLine($"Error processing ProcessID={process.Id}: {ex.Message}");
            }
        }
    }

    public static string GetProcessPathWMI(int processId)
    {
        string query = $"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {processId}";
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
        using (ManagementObjectCollection results = searcher.Get())
        {
            foreach (ManagementObject obj in results)
            {
                return obj["ExecutablePath"]?.ToString();
            }
        }
        return null;
    }


    public static string GetProcessPath(int processId)
    {
        try
        {
            using (var process = Process.GetProcessById(processId))
            {
                // Check for architecture compatibility
                if (!IsArchitectureCompatible(process))
                {
                    return "Cannot access modules of a process with a different architecture.";
                }

                // Get the process's main module file name
                return process.MainModule.FileName;
            }
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return $"Access denied: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private static bool IsArchitectureCompatible(Process process)
    {
        IntPtr handle = Kernel32NativeMethods.OpenProcess(NativeMethodConstants.PROCESS_QUERY_INFORMATION, false, process.Id);
        if (handle == IntPtr.Zero)
        {
            return false; // Cannot open process
        }

        try
        {
            bool isTargetWow64;
            bool isCurrentWow64 = Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess;

            if (Kernel32NativeMethods.IsWow64Process(handle, out isTargetWow64))
            {
                return isCurrentWow64 == isTargetWow64;
            }

            return false; // Cannot determine architecture
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(handle);
        }
    }

    public static string GetProcessImagePath(int processId)
    {
        IntPtr hProcess = Kernel32NativeMethods.OpenProcess(NativeMethodConstants.PROCESS_QUERY_INFORMATION | NativeMethodConstants.PROCESS_VM_READ, false, processId);

        if (hProcess == IntPtr.Zero)
        {
            return $"Error: Unable to open process ID {processId}.";
        }

        try
        {
            StringBuilder buffer = new StringBuilder(1024);
            int size = buffer.Capacity;

            if (Kernel32NativeMethods.QueryFullProcessImageName(hProcess, 0, buffer, ref size))
            {
                return buffer.ToString();
            }
            else
            {
                return $"Error: Unable to query process image for PID {processId}.";
            }
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(hProcess);
        }
    }


    private static uint GetParentProcessId(uint processId)
    {
        IntPtr snapshotHandle = Kernel32NativeMethods.CreateToolhelp32Snapshot(Flags.SnapshotFlags.Process, 0);
        if (snapshotHandle == IntPtr.Zero)
        {
            Console.WriteLine($"CreateToolhelp32Snapshot failed. Error: {Marshal.GetLastWin32Error()}");
            return 0;
        }

        try
        {
            Structs.PROCESSENTRY32 entry = new Structs.PROCESSENTRY32
            {
                dwSize = (uint)Marshal.SizeOf(typeof(Structs.PROCESSENTRY32))
            };

            if (Kernel32NativeMethods.Process32First(snapshotHandle, ref entry))
            {
                do
                {
                    if (entry.th32ProcessID == processId)
                    {
                        return entry.th32ParentProcessID;
                    }
                } while (Kernel32NativeMethods.Process32Next(snapshotHandle, ref entry));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetParentProcessId for ProcessID={processId}: {ex.Message}");
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(snapshotHandle);
        }

        return 0; // No parent process found
    }



    private static string GetProcessPublisher(string processPath)
    {
        try
        {
            if (File.Exists(processPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(processPath);
                return versionInfo.CompanyName ?? "Unknown Publisher";
            }
        }
        catch
        {
            // Ignore errors
        }
        return "Unknown Publisher";
    }

    public static string GetProcessName(int processId)
    {
        try
        {
            return Process.GetProcessById(processId).ProcessName;
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"Process with ID {processId} does not exist.");
            return "Unknown";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving process name for ProcessID={processId}: {ex.Message}");
            return "Unknown";
        }
    }


    public static void KRP(string fix, string hFixLog, string pro7, string pro5, string pro6, string err0, string pro4, string pro8)
    {
        // Process the file path using regular expressions
        string filePath1 = Regex.Replace(fix, @"\s<\d+>", "");
        filePath1 = Regex.Replace(filePath1, @"(?i)^\([^\(]*->\)\s*", "");
        filePath1 = Regex.Replace(filePath1, @"(?i)^\([^\(]*\)\s*(.+)", "$1");
        filePath1 = Regex.Replace(filePath1, @"(?i)^\[.+\]\s*(.+)", "$1");
        filePath1 = Regex.Replace(filePath1, @"\s+$", "");

        // Get the list of all running processes
        Process[] processes = Process.GetProcesses();
        bool processFound = false;

        foreach (var process in processes)
        {
            try
            {
                // Get the process path
                string processPath = GetProcessPath(process.Id);

                if (!string.IsNullOrEmpty(processPath) && filePath1.Equals(processPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the process is critical
                    if (!IsProcessCritical(process.Id))
                    {
                        File.AppendAllText(hFixLog, $"\"{filePath1}\" => {pro7}{Environment.NewLine}");
                        return;
                    }

                    processFound = true;

                    // Attempt to close the process
                    process.Kill();
                    process.WaitForExit();

                    File.AppendAllText(hFixLog, $"[{process.Id}] {filePath1} => {pro5}.{Environment.NewLine}");
                    break;
                }
            }
            catch (Exception ex)
            {
                // Handle specific errors
                string errorMessage;
                if (ex is InvalidOperationException)
                {
                    errorMessage = $"{filePath1} => {pro6}.{Environment.NewLine}";
                }
                else if (ex is UnauthorizedAccessException)
                {
                    errorMessage = $"{filePath1} => AdjustTokenPrivileges {err0}.{Environment.NewLine}";
                }
                else if (ex is SystemException)
                {
                    errorMessage = $"{filePath1} => {pro7}{Environment.NewLine}";
                }
                else
                {
                    errorMessage = $"{filePath1} => {pro4}{Environment.NewLine}";
                }


                File.AppendAllText(hFixLog, errorMessage);
                return;
            }
        }

        if (!processFound)
        {
            File.AppendAllText(hFixLog, $"{filePath1} => {pro8}{Environment.NewLine}");
        }
    }
    
  

    // Function to match certain process paths
    public static int ARG(string path = null)
    {
        if (path == null)
            path = "";  // Default empty path, adjust as needed.

        if (Regex.IsMatch(path, @"(?i)\b(shutdown|wevtutil|vssadmin|wmic|REGSVR|regsvr32|Regsvcs|RegAsm|regedt32|regedit|rundll32|wscript|cscript|javaw|cmd|powershell|reg)\b") ||
            Regex.IsMatch(path, @"(?i)java\.exe"))
        {
            return 1;
        }

        if (Regex.IsMatch(path, @"(?i)\b(userinit|dllhost|explorer)\b"))
        {
            return 2;
        }

        return 0;  // Return a default value (optional)
    }


    

    public static string GetProcessPathForNewerOS(int processId)
    {
        // For newer OS versions, use appropriate method to get the path
        try
        {
            var process = Process.GetProcessById(processId);
            return process.MainModule.FileName;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static void ProcessCritical(int processId, bool makeCritical)
    {
        IntPtr hProcess = Kernel32NativeMethods.OpenProcess((uint)Flags.ProcessAccessFlags.SetInformation, false, processId);
        if (hProcess == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to open process {processId}. Error: {Marshal.GetLastWin32Error()}");
        }

        try
        {
            int isCritical = makeCritical ? 1 : 0;
            int returnLength = 0;

            int status = NtdllNativeMethods.NtSetInformationProcess(
                hProcess,
                Structs.ProcessInformationClass.ProcessBreakOnTermination,
                ref isCritical,
                sizeof(int));

            if (status != 0) // STATUS_SUCCESS = 0
            {
                throw new InvalidOperationException($"Failed to set process critical status. NTSTATUS: {status}");
            }
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(hProcess);
        }
    }


    public static void ScanProcesses()
    {
        ProcessList.Clear();

        try
        {
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                string processPath = GetProcessPath(process.Id);

                if (!string.IsNullOrEmpty(processPath))
                {
                    ProcessList.Add($"<{process.Id}> {processPath}");
                }
                else if (!IsExcludedProcess(process))
                {
                    ProcessList.Add($"Process -> {process.ProcessName}");
                }
            }

            Console.WriteLine("\n==================== Processes =================");
            foreach (var process in ProcessList)
            {
                Console.WriteLine(process);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning processes: {ex.Message}");
        }
    }

    private static bool IsExcludedProcess(Process process)
    {
        string name = process.ProcessName;
        return Regex.IsMatch(name, @"(?i)\A((Secure |)system|Memory Compression|Registry|vmmem)$");
    }

    public static void ProcessTempFiles(StreamWriter frstLog, StreamWriter tempRp2)
    {
        var tempRpContent = File.ReadAllText(FolderConstants.TemprpPath);
        string[] tempRpLines = File.ReadAllLines(FolderConstants.TemprpPath);

        foreach (var line in tempRpLines)
        {
            string sanitizedPath = Regex.Replace(line, @"<.*>", "").Trim();
            string escapedPath = Regex.Escape(sanitizedPath);

            if (Regex.IsMatch(tempRpContent, escapedPath, RegexOptions.IgnoreCase))
                continue;

            tempRp2.WriteLine(line);

            int count = Regex.Matches(tempRpContent, escapedPath, RegexOptions.IgnoreCase).Count;
            string countTag = count > 1 ? $" <{count}>" : "";

            string parentProcessPath = GetParentProcessPath(sanitizedPath);

            string company = GetProcessPublisher(sanitizedPath);
            ProcessList.Add($"{parentProcessPath}{company} {sanitizedPath}{countTag}");
        }

        // Clean up temporary files
        File.Delete(FolderConstants.TemprpPath);
        File.Delete(FolderConstants.Temprp2Path);

        // Write results to FRST log
        frstLog.WriteLine("\n==================== Processes =================");
        foreach (var process in ProcessList)
        {
            Logger.Instance.LogPrimary(process);
        }
    }

    private static string GetParentProcessPath(string processPath)
    {
        // Implement logic to get parent process path
        return string.Empty;
    }

    // Get all running processes
    public static Process[] GetProcessList()
    {
        return Process.GetProcesses();
    }

    /// <summary>
    /// Checks if a process is marked as critical.
    /// </summary>
    /// <param name="process">The process to check.</param>
    /// <returns>True if the process is critical; otherwise, false.</returns>
    public static bool IsProcessCritical(int processId)
    {
        try
        {
            using (Process process = Process.GetProcessById(processId))
            {
                int isCritical = 0;
                int returnLength;

                int status = NtdllNativeMethods.NtQueryInformationProcess(
                    process.Handle,
                    NativeMethodConstants.ProcessBreakOnTermination,
                    ref isCritical,
                    sizeof(int),
                    out returnLength
                );

                if (status == 0) // STATUS_SUCCESS
                {
                    return isCritical != 0; // Non-zero indicates a critical process
                }
                else
                {
                    Console.WriteLine($"NtQueryInformationProcess failed with status code: {status}");
                }
            }
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"No process with ID {processId} found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if process is critical: {ex.Message}");
        }

        return false; // Default to non-critical if any error occurs
    }

    private static bool IsWhitelisted(string processPath)
    {
        // Debugging log
        Console.WriteLine($"Checking whitelist for: {processPath}");
        foreach (var whitelisted in ProcessWhitelist.Whitelist)
        {
            if (processPath.Equals(whitelisted, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Matched Whitelist: {whitelisted}");
                return true;
            }
        }
        return false;
    }

    private static bool IsBlacklisted(string processPath)
    {
        // Debugging log
        Console.WriteLine($"Checking blacklist for: {processPath}");
        foreach (var blacklisted in ProcessBlacklist.Blacklist)
        {
            if (processPath.Equals(blacklisted, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Matched Blacklist: {blacklisted}");
                return true;
            }
        }
        return false;
    }

}
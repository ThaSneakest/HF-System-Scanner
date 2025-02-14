using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scripting
{
    public class ProcessFix
    {
        private static string Fix = "FCheck: C:\\ExamplePath\\ExampleFile.txt [AdditionalInfo]";
        public static void SetProcessCritical(bool isCritical)
        {
            // Get the current process handle
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr processHandle = currentProcess.Handle;

            int breakOnTermination = isCritical ? 1 : 0;

            // Set the process as critical
            int status = NtdllNativeMethods.NtSetInformationProcess(
                processHandle,
                NativeMethodConstants.ProcessBreakOnTermination,
                ref breakOnTermination,
                sizeof(int));

            if (status != 0)
            {
                throw new InvalidOperationException($"Failed to set process critical status. NTSTATUS: 0x{status:X}");
            }

            Console.WriteLine($"Process {(isCritical ? "marked as critical" : "no longer critical")} successfully.");
        }

        public static void KILLDLL()
        {
            // Get the list of all processes
            Process[] processes = Process.GetProcesses();

            if (processes == null || processes.Length == 0)
                return; // No processes found, nothing to do

            // Iterate through each process
            foreach (var process in processes)
            {
                try
                {
                    // Check if the process name matches the specified pattern
                    if (Regex.IsMatch(process.ProcessName, "(?i)(dllhost|rundll32|regsvr32)"))
                    {
                        // Terminate the process
                        process.Kill();
                        Console.WriteLine($"Terminated process: {process.ProcessName} (ID: {process.Id})");
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions if a process cannot be terminated
                    Console.WriteLine($"Failed to terminate process: {process.ProcessName} (ID: {process.Id}). Error: {ex.Message}");
                }
            }
        }

        // Function to close unwanted processes
        public static void BBBBCP()
        {
            var processes = Process.GetProcesses().ToList();

            foreach (var process in processes)
            {
                string path = string.Empty;

                // Get the process path (adjust according to OS)
                if (Utility.GetOSVersion() < 6)
                {
                    path = ProcessUtils.GetProcessPath(process.Id); // Implement the method to get process path for older versions
                }
                else
                {
                    path = ProcessUtils.GetProcessPathForNewerOS(process.Id);  // Implement for newer versions
                }

                if (!string.IsNullOrEmpty(path))
                {
                    // Check for specific system processes that shouldn't be closed
                    if (!Regex.IsMatch(path, @"(?i)Windows\\System32\\(smss|csrss|wininit|csrss|services|lsass|lsm|winlogon|svchost|fontdrvhost|spoolsv|dwm|WUDFHost|msdtc|VSSVC|earchIndexer|SearchProtocolHost|SearchFilterHost|alg|sihost)\.exe") &&
                        !Regex.IsMatch(path, @"(?i)Windows\\explorer.exe") &&
                        !Regex.IsMatch(path, @"(?i)System32\\wbem\\WmiPrvSE\.exe") &&
                        !Regex.IsMatch(path, @"(?i)Windows\\SystemApps\\.+\\(SearchUI|RemindersServer|ActionUriServer)\.exe") &&
                        !path.Contains(Fix))
                    {
                        if (ProcessUtils.IsProcessCritical(process.Id))  // Implement method to check if process is critical
                        {
                            Process.GetProcessById(process.Id).Kill();  // Kill the process
                        }
                    }
                }
            }

            File.WriteAllText(@"C:\FRST\re", "R" + Environment.NewLine);
            File.AppendAllText(@"C:\Path\to\logfile.txt", "Log Entry" + Environment.NewLine);  // Replace with actual log path
        }

        private static void SetProcessCritical(int processId, bool makeCritical)
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

        public static void CloseProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                process.WaitForExit(); // Wait for the process to exit
                Console.WriteLine($"Closed process: {process.ProcessName} (ID: {processId})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to close process with ID {processId}: {ex.Message}");
            }
        }

    }
}

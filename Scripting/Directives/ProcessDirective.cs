using System;
using System.Diagnostics;
using System.IO;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ProcessDirective
    {
        public static void ProcessKill()
        {
            string filePath = "commands.txt"; // Path to your text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);
                bool processMode = false;

                foreach (string line in lines)
                {
                    if (line.StartsWith("Process::"))
                    {
                        processMode = true;
                        continue; // Skip the "Process::" line
                    }

                    if (line.StartsWith("StopFix::"))
                    {
                        Console.WriteLine("StopFix:: found. Stopping execution.");
                        break;
                    }

                    if (processMode)
                    {
                        string processPath = line.Trim();

                        if (!string.IsNullOrEmpty(processPath))
                        {
                            KillProcessByPath(processPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static void KillProcessByPath(string processPath)
        {
            try
            {
                Process[] allProcesses = Process.GetProcesses();

                foreach (var process in allProcesses)
                {
                    try
                    {
                        string processFilePath = GetProcessFilePath(process);
                        if (string.Equals(processFilePath, processPath, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Killing process: {process.ProcessName} (ID: {process.Id})");
                            process.Kill();
                            process.WaitForExit();
                            Console.WriteLine($"Process {process.ProcessName} terminated.");
                        }
                    }
                    catch
                    {
                        // Ignoring processes that cannot be accessed (e.g., system processes).
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing process by path '{processPath}': {ex.Message}");
            }
        }

        private static string GetProcessFilePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
                // Access to process.MainModule might throw an exception for some processes.
                return string.Empty;
            }
        }
    }
}

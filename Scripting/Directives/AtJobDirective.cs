using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class AtJobDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("AtJob::"))
                    {
                        Console.WriteLine("AtJob:: directive found. Fixing AtJob infection.");
                        FixAtJob();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void FixAtJob()
        {
            try
            {
                // Step 1: Check for suspicious tasks
                CheckForSuspiciousTasks();

                // Step 2: Remove any malicious tasks
                RemoveMaliciousTasks();

                // Step 3: Log success
                Console.WriteLine("AtJob infection fixed successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify tasks or files. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing AtJob infection: {ex.Message}");
            }
        }

        private static void CheckForSuspiciousTasks()
        {
            try
            {
                // Example task names or conditions to look for (customize based on malware behavior)
                string[] suspiciousTasks = new string[]
                {
                    "MaliciousTask1",
                    "MaliciousTask2",
                    "FakeUpdateTask"
                };

                foreach (var task in suspiciousTasks)
                {
                    // Check if the task exists in the Task Scheduler
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Query /TN {task}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            Console.WriteLine($"Found suspicious task: {task}");
                        }
                        else
                        {
                            Console.WriteLine($"No suspicious task found: {task}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for suspicious tasks: {ex.Message}");
            }
        }

        private static void RemoveMaliciousTasks()
        {
            try
            {
                // Example list of known malicious tasks to remove (customize as necessary)
                string[] maliciousTasks = new string[]
                {
                    "MaliciousTask1",
                    "MaliciousTask2",
                    "FakeUpdateTask"
                };

                foreach (var task in maliciousTasks)
                {
                    // Remove the suspicious task from Task Scheduler
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Delete /TN {task} /F", // /F forces deletion without confirmation
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"Successfully removed task: {task}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to remove task: {task}");
                            Console.WriteLine(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing malicious tasks: {ex.Message}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class StepDeleteDirective
    {
        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to your directives text file

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
                    if (line.StartsWith("StepDelete::"))
                    {
                        Console.WriteLine("StepDelete:: directive found. Ensuring all targeted processes are terminated...");
                        EnsureProcessesTerminated();
                        continue;
                    }

                    // Add additional directive handling logic here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void EnsureProcessesTerminated()
        {
            // Define a list of processes to be terminated
            string[] targetProcesses = { "notepad", "calc", "chrome" }; // Add process names as needed
            bool allTerminated = false;

            while (!allTerminated)
            {
                allTerminated = true;

                foreach (var processName in targetProcesses)
                {
                    var processes = Process.GetProcessesByName(processName);
                    if (processes.Length > 0)
                    {
                        allTerminated = false;
                        foreach (var process in processes)
                        {
                            try
                            {
                                Console.WriteLine($"Terminating process: {process.ProcessName} (ID: {process.Id})");
                                process.Kill();
                                process.WaitForExit();
                                Console.WriteLine($"Process {process.ProcessName} terminated.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to terminate process {process.ProcessName}: {ex.Message}");
                            }
                        }
                    }
                }

                if (!allTerminated)
                {
                    Console.WriteLine("Some processes are still running. Retrying...");
                    Thread.Sleep(1000); // Wait before retrying
                }
            }

            Console.WriteLine("All targeted processes have been terminated. Proceeding with the next directive.");
        }
    }
}

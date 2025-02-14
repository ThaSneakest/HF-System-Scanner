using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class TaskDetailsDirective
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
                    if (line.StartsWith("TaskDetails::"))
                    {
                        Console.WriteLine("TaskDetails:: directive found. Listing additional task details.");
                        ListTaskDetails();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ListTaskDetails()
        {
            try
            {
                Console.WriteLine("Fetching scheduled task details...");

                string query = "SELECT * FROM Win32_ScheduledJob";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject task in searcher.Get())
                    {
                        Console.WriteLine("Task Name: " + task["JobId"]);
                        Console.WriteLine("Command: " + task["Command"]);
                        Console.WriteLine("Scheduled Start Time: " + task["StartTime"]);
                        Console.WriteLine("Status: " + (task["Status"] ?? "Unknown"));
                        Console.WriteLine("Elapsed Time: " + task["ElapsedTime"]);
                        Console.WriteLine("=======================================");
                    }
                }

                Console.WriteLine("Scheduled task details fetched successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to list task details. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching task details: {ex.Message}");
            }
        }
    }
}

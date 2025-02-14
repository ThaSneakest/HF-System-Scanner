using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ClearEventLogDirective
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
                    if (line.StartsWith("ClearEventLogs::"))
                    {
                        Console.WriteLine("ClearEventLogs:: directive found. Clearing all event logs.");
                        ClearAllEventLogs();
                        break; // ClearEventLogs:: should execute only once
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ClearAllEventLogs()
        {
            try
            {
                EventLog[] eventLogs = EventLog.GetEventLogs();

                foreach (EventLog log in eventLogs)
                {
                    try
                    {
                        Console.WriteLine($"Clearing event log: {log.LogDisplayName}");
                        log.Clear();
                        Console.WriteLine($"Successfully cleared: {log.LogDisplayName}");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Access denied: Unable to clear log '{log.LogDisplayName}'. Run as administrator.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error clearing log '{log.LogDisplayName}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enumerating event logs: {ex.Message}");
            }
        }
    }
}

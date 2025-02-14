using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class SystemRestoreDirective
    {
        private const string LogFilePath = "WSS_Log.txt"; // Path to the log file

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
                    if (line.StartsWith("SystemRestore::"))
                    {
                        Console.WriteLine("SystemRestore:: directive found. Creating system restore point...");
                        CreateSystemRestorePoint();
                        continue;
                    }

                    // Add additional directive handling here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                LogToFile($"Error: {ex.Message}");
            }
        }

        private static void CreateSystemRestorePoint()
        {
            try
            {
                // Use WMI to create a system restore point
                ManagementClass managementClass = new ManagementClass("SystemRestore");

                var inParams = managementClass.GetMethodParameters("CreateRestorePoint");
                inParams["Description"] = "WSS Restore Point";
                inParams["RestorePointType"] = 12; // APPLICATION_INSTALL
                inParams["EventType"] = 100; // BEGIN_SYSTEM_CHANGE

                var result = managementClass.InvokeMethod("CreateRestorePoint", inParams, null);

                if (result != null && Convert.ToInt32(result) == 0)
                {
                    Console.WriteLine("System restore point created successfully.");
                    LogToFile("System restore point created successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to create system restore point.");
                    LogToFile("Failed to create system restore point.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while creating system restore point: {ex.Message}");
                LogToFile($"Error while creating system restore point: {ex.Message}");
            }
        }

        private static void LogToFile(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.AppendAllText(LogFilePath, $"{timestamp} - {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}

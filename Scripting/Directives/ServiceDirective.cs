using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ServiceDirective
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
                    if (line.StartsWith("Service::"))
                    {
                        string serviceName = line.Substring("Service::".Length).Trim();
                        if (!string.IsNullOrEmpty(serviceName))
                        {
                            Console.WriteLine($"Service:: directive found. Disabling service/driver: {serviceName}");
                            DisableService(serviceName);
                        }
                        else
                        {
                            Console.WriteLine("Service:: directive found, but no service name was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DisableService(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    // Stop the service if it is running
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine($"Stopping service: {serviceName}");
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        Console.WriteLine($"Service stopped: {serviceName}");
                    }

                    // Disable the service using sc.exe
                    Console.WriteLine($"Disabling service: {serviceName}");
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"config \"{serviceName}\" start= disabled",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process process = Process.Start(psi);
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"Service successfully disabled: {serviceName}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to disable service: {serviceName}. Exit code: {process.ExitCode}");
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Service not found: {serviceName}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to disable service '{serviceName}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disabling service '{serviceName}': {ex.Message}");
            }
        }
    }
}

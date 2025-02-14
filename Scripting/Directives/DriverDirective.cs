using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class DriverDirective
    {
        private const string ServicesKeyPath = @"SYSTEM\CurrentControlSet\Services";

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
                    if (line.StartsWith("Driver::"))
                    {
                        string serviceName = line.Substring("Driver::".Length).Trim();
                        if (!string.IsNullOrEmpty(serviceName))
                        {
                            Console.WriteLine($"Driver:: directive found. Deregistering and deleting driver service: {serviceName}");
                            DeregisterAndDeleteDriverService(serviceName);
                        }
                        else
                        {
                            Console.WriteLine("Driver:: directive found, but no service name was specified.");
                        }
                    }

                    // Add additional directive handling logic here.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DeregisterAndDeleteDriverService(string serviceName)
        {
            try
            {
                // Stop and delete the service
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.Paused)
                    {
                        Console.WriteLine($"Stopping service: {serviceName}");
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        Console.WriteLine($"Service {serviceName} stopped.");
                    }

                    // Delete the service using sc.exe
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "sc.exe",
                            Arguments = $"delete {serviceName}",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    Console.WriteLine($"Service {serviceName} deleted successfully.");
                }

                // Remove registry entry for the service
                using (RegistryKey servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ServicesKeyPath, writable: true))
                {
                    if (servicesKey?.OpenSubKey(serviceName) != null)
                    {
                        servicesKey.DeleteSubKeyTree(serviceName);
                        Console.WriteLine($"Service {serviceName} deregistered from the registry.");
                    }
                    else
                    {
                        Console.WriteLine($"Service {serviceName} not found in the registry.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deregistering and deleting driver service '{serviceName}': {ex.Message}");
            }
        }
    }
}

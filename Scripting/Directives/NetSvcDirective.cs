using System;
using System.IO;
using System.ServiceProcess;
using Microsoft.Win32;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class NetSvcDirective
    {
        private const string SvchostKeyPath = @"SYSTEM\CurrentControlSet\Services";

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
                    if (line.StartsWith("NetSvc::"))
                    {
                        string serviceName = line.Substring("NetSvc::".Length).Trim();
                        if (!string.IsNullOrEmpty(serviceName))
                        {
                            Console.WriteLine($"NetSvc:: directive found. Deregistering service: {serviceName}");
                            DeregisterService(serviceName);
                        }
                        else
                        {
                            Console.WriteLine("NetSvc:: directive found, but no service name was specified.");
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

        private static void DeregisterService(string serviceName)
        {
            try
            {
                // Stop the service if it's running
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.Paused)
                    {
                        Console.WriteLine($"Stopping service: {serviceName}");
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        Console.WriteLine($"Service {serviceName} stopped.");
                    }
                }

                // Remove the registry entry for the service
                using (RegistryKey servicesKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(SvchostKeyPath, writable: true))
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
                Console.WriteLine($"Error deregistering service '{serviceName}': {ex.Message}");
            }
        }
    }
}

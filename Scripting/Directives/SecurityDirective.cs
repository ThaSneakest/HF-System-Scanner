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
    public class SecurityDirective
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
                    if (line.StartsWith("Security::"))
                    {
                        string securitySoftware = line.Substring("Security::".Length).Trim();
                        if (!string.IsNullOrEmpty(securitySoftware))
                        {
                            Console.WriteLine($"Security:: directive found. Disabling: {securitySoftware}");
                            DisableSecuritySoftware(securitySoftware);
                        }
                        else
                        {
                            Console.WriteLine("Security:: directive found, but no software name was specified.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void DisableSecuritySoftware(string softwareName)
        {
            try
            {
                // Disable related services
                DisableService(softwareName);

                // Kill related processes
                KillProcessesByName(softwareName);

                Console.WriteLine($"Security software '{softwareName}' has been disabled.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied: Unable to disable '{softwareName}'. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disabling '{softwareName}': {ex.Message}");
            }
        }

        private static void DisableService(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine($"Stopping service: {serviceName}");
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        Console.WriteLine($"Service stopped: {serviceName}");
                    }

                    Console.WriteLine($"Disabling service: {serviceName}");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"config \"{serviceName}\" start= disabled",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    })?.WaitForExit();
                    Console.WriteLine($"Service disabled: {serviceName}");
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Service '{serviceName}' not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error managing service '{serviceName}': {ex.Message}");
            }
        }

        private static void KillProcessesByName(string processName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    Console.WriteLine($"No running processes found for: {processName}");
                    return;
                }

                foreach (var process in processes)
                {
                    Console.WriteLine($"Killing process: {process.ProcessName} (ID: {process.Id})");
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"Process terminated: {process.ProcessName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error terminating process '{processName}': {ex.Message}");
            }
        }
    }
}

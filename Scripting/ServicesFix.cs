using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner.Scripting
{
    public class ServicesFix
    {
        public static void Disable(string fix, string hFixLog, string diss, string nDiss)
        {

            try
            {
                // Extract the service name from the string using regex
                string serviceName = Regex.Replace(fix, @"(?i)DisableService:[ ]*(.+)", "$1", RegexOptions.IgnoreCase);

                string serviceKeyPath = $@"HKLM\SYSTEM\CurrentControlSet\ControlSet001\Services\{serviceName}";
                string serviceStartValuePath = $@"SYSTEM\CurrentControlSet\ControlSet001\Services\\{serviceName}";

                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(serviceStartValuePath, writable: true))
                {
                    if (key != null)
                    {
                        object startValue = key.GetValue("Start");
                        if (startValue != null)
                        {
                            // Change "Start" value to 4 (disable service)
                            key.SetValue("Start", 4, RegistryValueKind.DWord);

                            // Log success
                            File.AppendAllText(hFixLog, $"{serviceName} => {diss}{Environment.NewLine}");
                        }
                        else
                        {
                            // Log if "Start" value could not be opened
                            File.AppendAllText(hFixLog, $"{serviceName} => \\Start value could not be opened{Environment.NewLine}");
                        }
                    }
                    else
                    {
                        // Handle service not found
                        File.AppendAllText(hFixLog, $"{serviceName} => \\Service not found{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                File.AppendAllText(hFixLog, $"{ex.Message}{Environment.NewLine}");
            }
        }

        /*/
        public static void DELSERVICE(string FIX, string SYSTEM, string BOOTSYSTEM, string DEF, string HFIXLOG, string SERV, string NFOUND, string STOPS, string NSTOPS, string DELETED, Action<string> LUFILDEL)
        {
            string SKEY = Regex.Replace(FIX, "(?i)^[SRU]\\d ([^;]+);.*", "$1");

            switch (SKEY)
            {
                case "Themes":
                    string KEY = "HKLM\\" + SYSTEM + BOOTSYSTEM + DEF + "\\Services\\Themes";
                    RegistryValueHandler.DeleteRegistryValue(KEY, "DependOnService");
                    break;

                default:
                    KEY = "HKLM\\" + SYSTEM + BOOTSYSTEM + DEF + "\\Services\\" + SKEY;
                    if (!VAR(KEY))
                    {
                        File.AppendAllText(HFIXLOG, SKEY + " => " + SERV + " " + NFOUND + ".\n");
                        return;
                    }

                    if (SystemConstants.BootMode != "Recovery" && ServiceHandler.GetServiceStatus(SKEY) == "R")
                    {
                        StopService(SKEY);

                        try
                        {
                            if (ServiceHandler.GetServiceStatus(SKEY) != "S")
                            {
                                Thread.Sleep(2500); // Sleep for 2.5 seconds
                            }

                            if (ServiceHandler.GetServiceStatus(SKEY) != "S")
                            {
                                Thread.Sleep(2500); // Sleep for 2.5 seconds again
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle any potential errors while stopping the service
                            Console.WriteLine($"Error stopping service: {ex.Message}");
                        }

                        if (ServiceHandler.GetServiceStatus(SKEY) == "S")
                        {
                            File.AppendAllText(HFIXLOG, $"{SKEY} => {STOPS}.\n");
                        }
                        else
                        {
                            File.AppendAllText(HFIXLOG, $"{SKEY} => {NSTOPS}.\n");
                        }
                    }

                    RegistryKeyHandler.DeleteRegistryKey(KEY);
                    if (!VAR(KEY))
                    {
                        if (SystemConstants.BootMode != "Recovery" && ServiceHandler.GetServiceStatus(SKEY) == "R")
                            File.AppendAllText(FolderConstants.HomeDrive + "\\FRST\\re", "R");

                        File.AppendAllText(HFIXLOG, SKEY + " => " + SERV + " " + DELETED + "\n");
                    }

                    if (SKEY == "iThemes5")
                    {
                        KEY = @"HKLM\" + SYSTEM + BOOTSYSTEM + DEF + @"\Services\Themes";

                        try
                        {
                            // Assuming RegRead is a method that reads from the registry
                            var value = RegistryUtils.RegRead(KEY, "DependOnService");

                            // If the value is not null or empty, proceed to delete the value
                            if (!string.IsNullOrEmpty(value.ToString()))
                            {
                                RegistryValueHandler.DeleteRegistryValue(KEY, "DependOnService");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle any error that occurs during registry reading
                            Console.WriteLine($"Error occurred while reading registry: {ex.Message}");
                        }
                    }

                    // Calling the LUFILDEL delegate method
                    LUFILDEL(SKEY);
                    break;
            }
        }

        /*/
        public static int StopService(string serviceName)
        {
            const string SERVICES_ACTIVE_DATABASE = null; // Use the default database

            IntPtr scmHandle = Advapi32NativeMethods.OpenSCManagerW(null, SERVICES_ACTIVE_DATABASE, NativeMethodConstants.SC_MANAGER_CONNECT);
            if (scmHandle == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open service manager.");
                return ErrorHandler.SetError(1, 0, 0);
            }

            IntPtr serviceHandle = Advapi32NativeMethods.OpenServiceW(scmHandle, serviceName, NativeMethodConstants.SERVICE_STOP);
            Advapi32NativeMethods.CloseServiceHandle(scmHandle); // Close SCM handle
            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine($"Failed to open service: {serviceName}");
                return ErrorHandler.SetError(2, 0, 0);
            }

            Structs.SERVICE_STATUS serviceStatus = new Structs.SERVICE_STATUS();
            bool success = Advapi32NativeMethods.ControlService(serviceHandle, NativeMethodConstants.SERVICE_CONTROL_STOP, ref serviceStatus);
            Advapi32NativeMethods.CloseServiceHandle(serviceHandle); // Close service handle
            if (!success)
            {
                Console.WriteLine($"Failed to stop service: {serviceName}");
                return ErrorHandler.SetError(3, 0, 0);
            }

            Console.WriteLine($"Service {serviceName} stopped successfully.");
            return 0; // Success
        }

        public static bool VAR(string key)
        {
            using (var registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key))
            {
                return registryKey != null;
            }
        }

        public static void StartService(string serviceName, string computerName = "")
        {
            IntPtr hSCManager = Advapi32NativeMethods.OpenSCManager(computerName, NativeMethodConstants.SERVICES_ACTIVE_DATABASE, 1);
            if (hSCManager == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to open service manager.");
            }

            IntPtr hService = Advapi32NativeMethods.OpenService(hSCManager, serviceName, 16);
            if (hService == IntPtr.Zero)
            {
                Advapi32NativeMethods.CloseServiceHandle(hSCManager);
                throw new InvalidOperationException("Failed to open service.");
            }

            try
            {
                if (!Advapi32NativeMethods.StartService(hService, 0, IntPtr.Zero))
                {
                    uint error = Kernel32NativeMethods.GetLastError();
                    throw new InvalidOperationException($"StartService failed with error: {error}");
                }
            }
            finally
            {
                Advapi32NativeMethods.CloseServiceHandle(hService);
                Advapi32NativeMethods.CloseServiceHandle(hSCManager);
            }
        }
    }
}

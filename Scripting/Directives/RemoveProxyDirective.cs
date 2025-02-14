using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class RemoveProxyDirective
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
                    if (line.StartsWith("RemoveProxy::"))
                    {
                        Console.WriteLine("RemoveProxy:: directive found. Resetting proxy settings to default.");
                        ResetProxySettings();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ResetProxySettings()
        {
            try
            {
                // Reset proxy settings for the current user
                using (RegistryKey userKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
                {
                    if (userKey != null)
                    {
                        userKey.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                        userKey.DeleteValue("ProxyServer", false);
                        userKey.DeleteValue("AutoConfigURL", false);
                        Console.WriteLine("Proxy settings reset for the current user.");
                    }
                }

                // Reset proxy settings for the local machine (optional)
                using (RegistryKey machineKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
                {
                    if (machineKey != null)
                    {
                        machineKey.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                        machineKey.DeleteValue("ProxyServer", false);
                        machineKey.DeleteValue("AutoConfigURL", false);
                        Console.WriteLine("Proxy settings reset for the local machine.");
                    }
                }

                // Notify the system of the changes
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

                Console.WriteLine("Proxy settings reset successfully.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied: Unable to modify proxy settings. Run as administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting proxy settings: {ex.Message}");
            }
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;
    }
}

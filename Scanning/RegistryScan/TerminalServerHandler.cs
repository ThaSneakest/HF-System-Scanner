using System;
using Microsoft.Win32;

//Tested and working

namespace Wildlands_System_Scanner
{
    public class TerminalServerHandler
    {
        public static void ScanTerminalServer()
        {
            try
            {
                // Define the registry path for the Terminal Server key
                string keyPath = @"SYSTEM\CurrentControlSet\Control\Terminal Server";

                // Open the registry key
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null)
                    {
                        Logger.Instance.LogPrimary($"Registry key not found: HKLM\\{keyPath}");
                        return;
                    }

                    // Retrieve the fDenyTSConnections value
                    object valueData = key.GetValue("fDenyTSConnections");

                    if (valueData != null && int.TryParse(valueData.ToString(), out int denyTsConnections))
                    {
                        // Log if the value is not "1" (meaning Remote Desktop is enabled)
                        if (denyTsConnections != 1)
                        {
                            Logger.Instance.LogPrimary($"Terminal Server Key: HKLM\\{keyPath}, fDenyTSConnections = {denyTsConnections} (Remote Desktop is enabled)");
                        }
                        else
                        {
                            Logger.Instance.LogPrimary($"Terminal Server Key: HKLM\\{keyPath}, fDenyTSConnections = {denyTsConnections} (Remote Desktop is disabled)");
                        }
                    }
                    else
                    {
                        Logger.Instance.LogPrimary($"fDenyTSConnections value not found or invalid in key: HKLM\\{keyPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ScanTerminalServer: {ex.Message}");
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public class ProxyEnableFixHandler
{
    // The function implementation
    public static void ProxyEnableFix(string fix)
    {
        try
        {
            // Extract user information from the fix string
            string user = System.Text.RegularExpressions.Regex.Replace(fix, @"ProxyEnable: \[(.+?)\] =>.*", "$1");
            string key1 = @"\Microsoft\Windows\CurrentVersion\Internet Settings";
            string key = @"HKU\" + user + @"\Software" + key1;
            string val = "ProxyEnable";

            // Check for HKLM (local machine)
            if (user == "HKLM")
            {
                key = @"HKLM\Software" + key1;
            }

            // Delete the ProxyEnable registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Call InternetSetOption to disable the proxy (assuming the flag 39 disables the proxy setting)
            WininetNativeMethods.InternetSetOption(0, 39, null, 0);
            Console.WriteLine("Proxy settings fixed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProxyEnableFix: {ex.Message}");
        }
    }
}
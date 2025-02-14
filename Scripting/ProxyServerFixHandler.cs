using System;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

public class ProxyServerFixHandler
{
    // The function implementation
    public static void ProxyServerFix(string fix)
    {
        try
        {
            // Extract user information from the fix string
            string user = Regex.Replace(fix, @"ProxyServer: \[(.+?)\] =>.*", "$1");
            string key1 = @"\Microsoft\Windows\CurrentVersion\Internet Settings";
            string key = @"HKU\" + user + @"\Software" + key1;
            string val = "ProxyServer";

            // Check for HKLM (local machine)
            if (user == "HKLM")
            {
                key = @"HKLM\Software" + key1;
            }

            // Delete the ProxyServer registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);
            Console.WriteLine("Proxy server settings fixed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProxyServerFix: {ex.Message}");
        }
    }
}
using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

public class PerRouteHandler
{
    public static void PerRouteFunction(string fix)
    {
        try
        {
            // Extract the value from the FIX string using regular expression
            string val = Regex.Replace(fix, @"[^[]+:\s*\[([^\]]*)\].*", "$1");

            // Call the method to delete the registry value
            RegistryValueHandler.DeleteRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\PersistentRoutes", val);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during PerRoute: {ex.Message}");
        }
    }
}

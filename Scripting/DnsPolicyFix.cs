using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

public class DnsPolicyFix
{
    public static void HandleDnsPolicyFix(string fix)
    {
        // Extract the key from the fix string using regular expression
        string key = Regex.Replace(fix, @"DnsPolicyConfig: \[(.+?)\] =>.*", "$1");

        // Define the registry path to delete
        string registryKey = $@"HKLM\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters\DnsPolicyConfig\{key}";

        // Delete the registry key
        RegistryKeyHandler.DeleteRegistryKey(registryKey);
    }
}

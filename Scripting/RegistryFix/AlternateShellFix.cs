using System;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

public class AlternateShellFix
{
    // Method to restore the AlternateShell registry value
    public static void RestoreAlternateShell()
    {
        string keyPath = $@"HKLM\{SystemRegistryPath}\Control\SafeBoot";
        string valueName = "AlternateShell";
        string valueData = "cmd.exe";

        // Call the method to restore the value in the registry
        RegistryValueHandler.RestoreRegistryValue(keyPath, valueName, valueData);
    }
    // Placeholder for SystemRegistryPath, you should adjust this based on your system's context
    private static string SystemRegistryPath = "Software\\Microsoft\\Windows NT\\CurrentVersion";
}

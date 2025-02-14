using System;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

public class BootExecuteFix
{
    // Method to restore the BootExecute registry value
    public static void BOOTEXECUTEFIX()
    {
        string keyPath = @"HKLM\" + SYSTEM + BOOTSYSTEM + DEF + @"\Control\Session Manager";
        string valueName = "BootExecute";
        string valueData = "autocheck autochk *";

        RegistryValueHandler.RestoreRegistryValue(keyPath, valueName, valueData);
    }

    // Sample system configuration variables (replace with actual values)
    private static string SYSTEM = "SYSTEM"; // Example, replace with actual value
    private static string BOOTSYSTEM = "BOOTSYSTEM"; // Example, replace with actual value
    private static string DEF = "DEF"; // Example, replace with actual value
}
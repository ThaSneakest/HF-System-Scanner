using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public class ProxyRemovalHandler
{


    // Method to remove proxy settings from specific registry keys
    private static void RemoveProxySettings(string key, string valueName)
    {
        try
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (registryKey != null && registryKey.GetValue(valueName) != null)
                {
                    registryKey.DeleteValue(valueName);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing proxy setting for key {key}: {ex.Message}");
        }
    }

    // Method to remove proxy settings from the system or user registry
    private static void RemoveProxyFromRegistry()
    {
        string key = @"HKLM\SOFTWARE\Policies\Microsoft\Internet Explorer";
        RegistryKeyHandler.DeleteRegistryKey(key);

        // Assuming USERREG is an array of user registry paths
        string[] userReg = { "user1", "user2" }; // Example user registry paths
        foreach (string user in userReg)
        {
            key = @"HKU\" + user + @"\SOFTWARE\Policies\Microsoft\Internet Explorer";
            RegistryKeyHandler.DeleteRegistryKey(key);
        }

        string key1 = @"\Microsoft\Windows\CurrentVersion\Internet Settings";
        RemoveProxySettings("HKLM\\SOFTWARE\\Policies" + key1, "ProxySettingsPerUser");
        RemoveProxySettings("HKLM\\SYSTEM\\CurrentControlSet\\services\\NlaSvc\\Parameters\\Internet\\ManualProxies", "");

        RemoveProxySettings("HKLM\\Software" + key1, "");
        key = @"HKLM\SOFTWARE" + key1 + @"\Connections";
        RemoveProxySettings(key, "DefaultConnectionSettings");
        RemoveProxySettings(key, "SavedLegacySettings");

        key = @"HKLM\SYSTEM\CurrentControlSet\Services\iphlpsvc\Parameters\ProxyMgr";
        var arrKey = new System.Collections.Generic.List<string>();

        using (RegistryKey hkey = Registry.LocalMachine.OpenSubKey(key))
        {
            if (hkey != null)
            {
                int i = 0;
                while (true)
                {
                    string subKeyName = hkey.GetSubKeyNames().Length > i ? hkey.GetSubKeyNames()[i] : null;
                    if (subKeyName == null) break;

                    string value = (string)hkey.OpenSubKey(subKeyName)?.GetValue("AutoConfigURL");
                    if (value != null)
                    {
                        arrKey.Add(key + "\\" + subKeyName);
                    }

                    i++;
                }
            }
        }

        foreach (var registryKey in arrKey)
        {
            RegistryKeyHandler.DeleteRegistryKey(registryKey);
        }

        foreach (string user in userReg)
        {
            RemoveProxySettings(@"HKU\" + user + @"\Software" + key1, "DefaultConnectionSettings");
            RemoveProxySettings(@"HKU\" + user + @"\Software" + key1, "SavedLegacySettings");
        }

        // Run commands to reset proxy settings
        System.Diagnostics.Process.Start("cmd.exe", "/c proxycfg -d");
        string bitsAdminPath = Path.Combine(Environment.SystemDirectory, "bitsadmin.exe");
        System.Diagnostics.Process.Start("cmd.exe", "/c " + bitsAdminPath + " /util /setieproxy localsystem NO_PROXY RESET");

        // Assuming there is a log function to write to log
        //Logger.WriteToLog(Logger.FixLog);
    }


    // Method to read a registry value and remove it if it exists
    public static void RemoveProxyCheck(string key, string value, bool ya = false)
    {
        try
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (registryKey != null)
                {
                    object registryValue = registryKey.GetValue(value);
                    if (registryValue != null)
                    {
                        // Delete the registry value
                        registryKey.DeleteValue(value);
                        // Call the InternetSetOption function to reset proxy settings
                        WininetNativeMethods.InternetSetOption(0, 39, null, 0);
                    }
                    else if (ya)
                    {
                        Console.WriteLine($"Value not found: {key}\\{value}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing proxy check: {ex.Message}");
        }
    }

    // Method to remove proxy settings from a specific registry key
    public static void RemoveProxySub(string key)
    {
        try
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key, writable: true))
            {
                if (registryKey != null)
                {
                    // Remove ProxyEnable if it exists
                    if (registryKey.GetValue("ProxyEnable") != null)
                    {
                        registryKey.DeleteValue("ProxyEnable");
                    }

                    // Remove ProxyServer if it exists
                    if (registryKey.GetValue("ProxyServer") != null)
                    {
                        registryKey.DeleteValue("ProxyServer");
                    }

                    // Remove AutoConfigURL if it exists
                    if (registryKey.GetValue("AutoConfigURL") != null)
                    {
                        registryKey.DeleteValue("AutoConfigURL");
                    }

                    // Call InternetSetOption to reset proxy settings
                    WininetNativeMethods.InternetSetOption(0, 39, null, 0);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing proxy settings: {ex.Message}");
        }
    }

}
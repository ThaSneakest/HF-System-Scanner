using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

public class AutoConfigUrlHandler
{
    public static void HandleAutoConfigUrlFix(string fix)
    {
        // Extract the User value from the fix string using regular expression
        string user = Regex.Replace(fix, @"AutoConfigURL: \[(.+?)\] =>.*", "$1");

        // Check if the user string is not "HKLM" and does not match a specific pattern
        if (!user.Contains("HKLM") && !Regex.IsMatch(user, @"^S-1-5-|^\.DEFAULT"))
        {
            string registryKey = $@"HKLM\SYSTEM\CurrentControlSet\Services\iphlpsvc\Parameters\ProxyMgr\{user}";
            RegistryKeyHandler.DeleteRegistryKey(registryKey);
        }

        // Define the registry path
        string key = $@"HKU\{user}\Software\Microsoft\Windows\CurrentVersion\Internet Settings";

        if (user == "HKLM")
        {
            key = @"HKLM\Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        }

        string value = "AutoConfigURL";

        // Delete the value in the registry
        RegistryValueHandler.DeleteRegistryValue(key, value);
    }
}

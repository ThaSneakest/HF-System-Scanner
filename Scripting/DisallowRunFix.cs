using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Registry;

public class DisallowRunFix
{
    public static void FixDisallowRun(string fix)
    {
        // Extract the user SID from the input string
        string userSid = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+", "$1", RegexOptions.IgnoreCase);
        RegistryUtils.RELOAD(userSid);

        // Registry key path for DisallowRun
        string keyPath = $@"HKU\{userSid}\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\DisallowRun";

        // Extract the value name to be deleted from the input string
        string valueName = Regex.Replace(fix, @"(?i)HKU\\.+\\Policies\\Explorer\\DisallowRun: \[(.*)\] .*", "$1", RegexOptions.IgnoreCase);

        // Delete the specified value from the registry key
        RegistryValueHandler.DeleteRegistryValue(keyPath, valueName);

        // Reapply settings after the operation using the existing RELOAD method
        RegistryUtils.RELOAD(userSid);
    }

}

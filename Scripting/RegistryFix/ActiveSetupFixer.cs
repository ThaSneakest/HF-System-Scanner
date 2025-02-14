using Microsoft.Win32;
using System;
using Wildlands_System_Scanner.Registry;

public class ActiveSetupFixer
{
    public static void HandleActiveSetupFix(string fix)
    {
        string key = @"HKLM\Software\Microsoft\Active Setup\Installed Components";
        string subKey = System.Text.RegularExpressions.Regex.Replace(fix, @".+?:\s*\[(.+?)\].*", "$1");

        RegistryKeyHandler.DeleteRegistryKey(key + @"\" + subKey);
    }
}

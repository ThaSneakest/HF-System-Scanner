using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Registry;

public class EnvironmentFix
{
    public void ENVHIJFIX(string FIX, string HFIXLOG)
    {
        // Regex replace to extract the value
        string VAL = Regex.Replace(FIX, @"(?i).+Environment: \[(.*)\] .*", "$1");

        // Regex replace to extract the key
        string KEY = Regex.Replace(FIX, @"(?i)(.+Environment):.*", "$1");

        // Regex to extract the user from FIX
        string USER = Regex.Replace(FIX, @"(?i)HKU\\([^\)]+)\.+", "$1");

        RegistryUtils.RELOAD(USER);

        RegistryValueHandler.DeleteRegistryValue(KEY, VAL);

        RegistryUtils.REUNLOAD(USER);
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;
using System.Drawing.Text;
using Wildlands_System_Scanner.Constants;

//Needs testing

public static class FilterHandler
{
    


    public static void LUFIL()
    {
        // Array of GUIDs
        string[] arrFil =
        {
        "{4D36E967-E325-11CE-BFC1-08002BE10318}",
        "{71A27CDD-812A-11D0-BEC7-08002BE2092F}",
        "{4D36E96B-E325-11CE-BFC1-08002BE10318}",
        "{4D36E96A-E325-11CE-BFC1-08002BE10318}",
        "{4D36E97B-E325-11CE-BFC1-08002BE10318}"
    };

        // Registry key base path
        string baseKeyPath = $@"HKLM\SYSTEM\CurrentControlSet\Control\Class\";

        // Get the OS version
        var osVersion = SystemConstants.OperatingSystemNumber.Version;

        // Process UpperFilters
        for (int i = 0; i < arrFil.Length; i++)
        {
            string keyPath = baseKeyPath + arrFil[i];
            string upperFilters = RegistryValueHandler.ReadRegistryValue(keyPath, "UpperFilters");

            if (string.IsNullOrEmpty(upperFilters))
            {
                if (i > 1) continue;
                if (i == 1 && osVersion.Major < 10 && osVersion > new Version(5, 1)) continue;
            }

            upperFilters = upperFilters?.Replace("\v", " ");

            if (i == 0 && upperFilters == "PartMgr") continue;
            if (i == 1 && upperFilters == "volsnap" && (osVersion.Major == 10 || osVersion == new Version(5, 1))) continue;
            if (i == 2 && upperFilters == "kbdclass") continue;

            Logger.Instance.LogPrimary($"UpperFilters: [{arrFil[i]}] -> [{upperFilters}]");
        }

        // Process LowerFilters
        for (int i = 0; i < arrFil.Length; i++)
        {
            string keyPath = baseKeyPath + arrFil[i];
            string lowerFilters = RegistryValueHandler.ReadRegistryValue(keyPath, "LowerFilters");

            if (string.IsNullOrEmpty(lowerFilters))
            {
                if (i > 1 || osVersion == new Version(5, 1)) continue;
                if (i == 0 && osVersion < new Version(6, 3)) continue;
            }

            lowerFilters = lowerFilters?.Replace("\v", " ");

            if (i == 0 && lowerFilters == "EhStorClass" && (osVersion.Major == 10 || osVersion == new Version(6, 3))) continue;
            if (i == 0 && lowerFilters == "iaStorF" && osVersion == new Version(6, 1)) continue;
            if (i == 1 && lowerFilters == "fvevol iorate rdyboost" && osVersion.Major == 10) continue;
            if (i == 1 && lowerFilters == "fvevol rdyboost" && (osVersion == new Version(6, 3) || osVersion == new Version(6, 1))) continue;
            if (i == 1 && lowerFilters == "ecache" && osVersion.Major == 6) continue;

            Logger.Instance.LogPrimary($"LowerFilters: [{arrFil[i]}] -> [{lowerFilters}]");
        }
    }


    public static void LUFILDEL(string system, string bootSystem, string def, string ser)
    {
        string baseKeyPath = $@"SYSTEM\{system}\{bootSystem}\{def}\Control\Class";

        using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(baseKeyPath, writable: true))
        {
            if (baseKey == null)
            {
                Console.WriteLine("Failed to open registry key.");
                return;
            }

            foreach (string subKeyName in baseKey.GetSubKeyNames())
            {
                string subKeyPath = $@"{baseKeyPath}\{subKeyName}";
                //LUFILDEL1(subKeyPath, "LowerFilters", ser, Logger.WildlandsLogFile); // Pass logFilePath as the 4th argument
               // LUFILDEL1(subKeyPath, "UpperFilters", ser, Logger.WildlandsLogFile); // Pass logFilePath as the 4th argument
            }

        }
    }

   
}

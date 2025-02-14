using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class SubFix
    {
        public static void Subfix()
        {
            string var1 = SubHandler.SUB();
            string key = @"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\SubSystems";

            if (!string.IsNullOrEmpty(var1))
            {
                RegistryValueHandler.RestoreRegistryValue(
                    RegistryHive.LocalMachine,   // Specify the target hive (e.g., HKEY_LOCAL_MACHINE)
                    key,                         // The registry key path
                    "Windows",                   // The name of the value
                    var1,                        // The value data
                    RegistryValueKind.ExpandString // Use the appropriate RegistryValueKind for REG_EXPAND_SZ
                );

            }
            else
            {
                //File.WriteAllText(Logger.WildlandsLogFile, key + @"\Windows => " + ERRSV + Environment.NewLine);
            }
        }
    }
}

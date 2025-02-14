using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting
{
    public class AppCertFix
    {
        public static void HandleAppCertFix(string fix)
        {
            // Extract registry value name using regex
            string val = Regex.Replace(fix, @"(?i)HKLM\\\.\.\.\\AppCertDlls: \[(.*)\] ->.*", "$1");

            string key = $@"HKLM\Software\Microsoft\Windows NT\CurrentVersion\Control\Session Manager\AppCertDlls";

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, val);
        }
    }
}

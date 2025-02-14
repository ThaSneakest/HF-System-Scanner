using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting
{
    public class BrowserHelperObjectFix
    {
        public static void BHOFix()
        {
            string clsid;
            string key = "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects\\";
            string regExpr = "HKLM\\Software\\Classes\\CLSID\\";

            clsid = Regex.Replace("FIX_STRING", @".+? -> (.+?) ->.*", "$1");
            key = key + clsid;

            // Simulating the deletion of a registry key
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key) != null)
            {
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(key);
            }

            key = regExpr + clsid;
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key) != null)
            {
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(key);
            }
        }
    }
}

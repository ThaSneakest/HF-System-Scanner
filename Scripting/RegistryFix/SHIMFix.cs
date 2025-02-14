using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class SHIMFix
    {
        public void SHIMFIX(string fix)
        {
            string wowKey = string.Empty;
            if (fix.Contains("\\Wow6432Node\\"))
            {
                wowKey = "\\Wow6432Node";
            }

            string key = System.Text.RegularExpressions.Regex.Replace(fix, @".+\\AppCompatFlags\\(.+?):.+", "$1");
            key = $@"HKLM\Software{wowKey}\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\{key}";

            RegistryKeyHandler.DeleteRegistryKey(key);
        }
    }
}

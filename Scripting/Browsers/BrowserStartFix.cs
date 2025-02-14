using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.Browsers
{
    public class BrowserStartFix
    {
        public static void FixBrowserStart(string fix)
        {
            string browser = Regex.Replace(fix, "StartMenuInternet: (.+?) -.*", "$1");
            string akey = @"HKLM\SOFTWARE\Clients\StartMenuInternet\" + browser + @"\shell\open\command";
            string data = string.Empty;

            if (browser.Contains("Chrome"))
            {
                data = "\"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google\\Chrome\\Application\\chrome.exe") + "\"";
            }
            else if (browser.Contains("Brave"))
            {
                data = "\"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "BraveSoftware\\Brave-Browser\\Application\\brave.exe") + "\"";
            }
            else if (browser.Contains("IEXPLORE"))
            {
                data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Internet Explorer\\iexplore.exe");
            }
            // Add other cases as necessary...

            if (!string.IsNullOrEmpty(akey))
            {
                RegistryValueHandler.RestoreRegistryValue(akey, "REG_SZ", data);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class SIOIFix
    {
        public void SIOIFIX(string fix)
        {
            string user = "", key = "", clsid = "", sub = "", key1 = "";

            // Extract the values from the FIX string using regex
            sub = Regex.Replace(fix, @"(?i).+?: \[(.+)\] -> .+ =>.*", "$1");
            clsid = Regex.Replace(fix, @"(?i).+?: .+ -> (.+) =>.*", "$1");
            key1 = @"HKLM\Software\Classes\CLSID\" + clsid;

            if (Regex.IsMatch(fix, @"(?i)Handlers\d_(S|.DEFAULT)"))
            {
                user = Regex.Replace(fix, @"ContextMenuHandlers\d_([^:]+):.+", "$1");
                key1 = @"HKU\" + user + @"\SOFTWARE\Classes\CLSID\" + clsid;
            }

            // Determine the registry key based on the fix string
            if (fix.Contains("Identifiers: "))
            {
                key = @"HKLM\Software\Microsoft\Windows\CurrentVersion\Explorer\ShellIconOverlayIdentifiers\" + sub;
            }
            else if (fix.Contains("ers1:"))
            {
                key = @"HKLM\Software\Classes\*\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("ers2:"))
            {
                key = @"HKLM\Software\Classes\Drive\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("ers3:"))
            {
                key = @"HKLM\Software\Classes\AllFileSystemObjects\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("ers4:"))
            {
                key = @"HKLM\Software\Classes\Directory\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("ers5:"))
            {
                key = @"HKLM\Software\Classes\Directory\Background\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("ers6:"))
            {
                key = @"HKLM\Software\Classes\Folder\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("Handlers1_"))
            {
                key = @"HKU\" + user + @"\Software\Classes\*\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("Handlers2_"))
            {
                key = @"HKU\" + user + @"\Software\Classes\Drive\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("Handlers3_"))
            {
                key = @"HKU\" + user + @"\Software\Classes\AllFileSystemObjects\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("Handlers4_"))
            {
                key = @"HKU\" + user + @"\Software\Classes\Directory\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("Handlers5_"))
            {
                key = @"HKU\" + user + @"\Software\Classes\Directory\Background\ShellEx\ContextMenuHandlers\" + sub;
            }
            else if (fix.Contains("Handlers6_"))
            {
                key = @"HKU\" + user + @"\Software\Classes\Folder\ShellEx\ContextMenuHandlers\" + sub;
            }

            // Delete the registry keys
            RegistryKeyHandler.DeleteRegistryKey(key);
            if (!string.IsNullOrEmpty(key1))
            {
                RegistryKeyHandler.DeleteRegistryKey(key1);
            }
        }
    }
}

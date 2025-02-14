using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Whitelist
{
    public class FolderWhitelist
    {
        public static readonly HashSet<string> WhitelistFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Add specific folder paths to the whitelist
            @"C:\Users\12565\Desktop\Wildlands System Scanner",
            @"C:\Users\12565\Desktop\Violet-v1.6\",
            @"C:\Users\12565\Desktop\pchfsa",
            @"C:\Users\12565\Desktop\Adobe Acrobat XI",
            @"C:\Users\12565\Desktop\$PLUGINSDIR",
            @"C:\Users\12565\Desktop\$0",
            @"C:\Windows\System32\CatRoot",
            @"C:\Windows\System32\config\systemprofile\AppData\Local\Microsoft\InstallService",
            @"C:\Windows\System32\config\systemprofile\AppData\LocalLow\Microsoft\CryptnetUrlCache",
            @"C:\Windows\System32\config\TxR",
            @"C:\Windows\System32\DriverStore\FileRepository",
            @"C:\Windows\System32\LogFiles\Scm",
            @"C:\Windows\System32\LogFiles\SQM",
            @"C:\Windows\System32\LogFiles\WMI",
            @"C:\Windows\System32\Microsoft\Protect",
            @"C:\Windows\System32\Msdtc",
            @"C:\Windows\System32\sysprep\Panther",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Application Experience",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\DiskDiagnostic",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Media Center",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\MobilePC",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Offline Files",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Setup\EOSNotify",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Setup\EOSNotify2",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\SideShow",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Wininet\CacheTask",
            @"C:\Windows\System32\wbem\AutoRecover",
            @"C:\Windows\System32\wdi",
            @"C:\Windows\System32\winevt",
            @"C:\Windows\System32\DRVSTORE",
            @"C:\Windows\System32\sysprep\Panther\IE",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\DiskDiagnostic",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\MobilePC",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Offline Files",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Setup",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\SideShow",
            @"C:\Windows\System32\Tasks\Microsoft\Windows\Wininet",
            @"C:\Windows\System32\config\RegBack\DEFAULT",
            @"C:\Windows\System32\config\RegBack\SAM",
            @"C:\Windows\System32\config\RegBack\SECURITY",
            @"C:\Windows\System32\config\RegBack\SOFTWARE",
            @"C:\Windows\System32\config\RegBack\SYSTEM",
        };
    }
}

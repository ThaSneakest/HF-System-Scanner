using DevExpress.Utils.DirectXPaint.Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Constants
{
    public class RegistryConstants
    {
        //Displays Windows 10 Pro on CPD Computer
        public static string RegistryOperatingSystem = (string)Microsoft.Win32.Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "ProductName",
            null);

        public static string GoogleChromeRegistryCurrentUserUninstallPath = (string)Microsoft.Win32.Registry.GetValue(
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\Google Chrome",
            "DisplayVersion",
            null);

        public static string GoogleChromeRegistryLocalMachineUninstallPath = (string)Microsoft.Win32.Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Uninstall\Google Chrome",
            "DisplayVersion",
            null);

        public static string FirefoxRegistryLocalMachineUninstallPath = (string)Microsoft.Win32.Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Uninstall\Mozilla Firefox",
            "DisplayVersion",
            null);

        public static string FirefoxRegistryCurrentUserUninstallPath = (string)Microsoft.Win32.Registry.GetValue(
            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\Mozilla Firefox",
            "DisplayVersion",
            null);

        public static string[] UserSubKeys = Microsoft.Win32.Registry.Users.GetSubKeyNames();

        public const string EventLogKeyPath = @"SYSTEM\CurrentControlSet\Services\Eventlog\";
        public const string AppPathsKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";
        public const string SessionManagerEnvironmentKey = @"System\CurrentControlSet\Control\Session Manager\Environment";
    }
}

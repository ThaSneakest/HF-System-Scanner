using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class WinlogonFix
    {
        public static void WinLogonSysFunc(string FIX)
        {
            string val = FIX.Contains("System") ? "System" : "Taskman";  // Equivalent to StringInStr

            string key = "HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon";

            RegistryValueHandler.DeleteRegistryValue(key, val);
        }

        public static void DeleteNotify(string software, string val)
        {
            // Define the registry key based on the passed values
            string key = @"HKLM\" + software + @"\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\" + val;

            // Call method to delete the registry key
            RegistryKeyHandler.DeleteRegistryKey(key);
        }
        public static void SENSLOGN()
        {
            string key = @"HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\SensLogn";
            string valueName = "DllName";
            string valueData = "wlnotify.dll";

            // Call the RestoreVal method to restore the registry value
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, key, valueName, valueData, RegistryValueKind.ExpandString);
        }
        public void SHELL()
        {
            RegistryValueHandler.RestoreRegistryValue(
                RegistryHive.LocalMachine,
                @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon",
                "Shell",
                "Explorer.exe",
                RegistryValueKind.String
            );

        }

        public void RestoreDimsntfy()
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\dimsntfy";
            const string valueName = "DllName";
            const string valueType = "REG_EXPAND_SZ";
            const string valueData = @"%SystemRoot%\System32\dimsntfy.dll";

            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKey, writable: true))
                {
                    if (key == null)
                    {
                        throw new Exception($"Registry key not found: {subKey}");
                    }

                    key.SetValue(valueName, valueData, RegistryValueKind.ExpandString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public static void USHELL(string fix)
        {
            string user;
            string key;
            string accountName;

            // Extract user from the FIX string using regular expressions
            user = System.Text.RegularExpressions.Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+Winlogon:.*", "$1");

            // Simulating the RELOAD function
            RegistryUtils.RELOAD(user);

            key = @"HKU\" + user + @"\Software\Microsoft\Windows NT\CurrentVersion\Winlogon";

            if (RegistryKeyHandler.CheckRegistryKeyLocked(key))
            {
                // Lookup the account name for the SID
                var securityIdentifier = new System.Security.Principal.SecurityIdentifier(user);
                var account = (System.Security.Principal.NTAccount)securityIdentifier.Translate(typeof(System.Security.Principal.NTAccount));
                accountName = account.Value;


                if (!string.IsNullOrEmpty(accountName))
                {
                    user = accountName;
                }

                // Unlock the registry key and give permissions
                int userId = int.Parse(user);
                SecurityUtils.Unlock(key, "4", userId);



            }

            string val = "Shell";
            RegistryValueHandler.DeleteRegistryValue(key, val);

            // Simulating REUNLOAD function
            RegistryUtils.REUNLOAD(user);
        }
        public static void Crypt32Chain()
        {
            string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\crypt32chain";
            string valueName = "DllName";
            string valueData = "crypt32.dll";

            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, valueName, valueData, RegistryValueKind.ExpandString);
        }
        public static void CryptNet()
        {
            string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\cryptnet";
            string valueName = "DllName";
            string valueData = "cryptnet.dll";

            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, valueName, valueData, RegistryValueKind.ExpandString);
        }

        public static void CscDll()
        {
            string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\cscdll";
            string valueName = "DllName";
            string valueData = "cscdll.dll";

            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, valueName, valueData, RegistryValueKind.ExpandString);
        }
        public void SCHEDULE()
        {
            // The registry key path
            string keyPath = @"HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\Schedule";

            // Call RESTOREVAL to set the registry value
            RegistryValueHandler.RestoreRegistryValue(
                RegistryHive.LocalMachine, // Specify the registry hive
                keyPath,                   // Registry key path
                "DllName",                 // Value name
                "wlnotify.dll",            // Value data
                RegistryValueKind.ExpandString // Correct enum for REG_EXPAND_SZ
            );

        }

        public void SCLGNTFY()
        {
            // The registry key path
            string keyPath = @"HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\sclgntfy";

            // Call RESTOREVAL to set the registry value
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, keyPath, "DllName", "wlnotify.dll", RegistryValueKind.ExpandString);
        }

        public void SetTermsrvDllName()
        {
            string keyPath = @"HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon\Notify\termsrv";
            string valueName = "DllName";
            string valueData = "wlnotify.dll";

            // Calling method to update registry value
            RegistryValueHandler.SetRegistryValue(keyPath, valueName, valueData);
        }

    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Scripting;

namespace Wildlands_System_Scanner
{
    public class FileExtensionHandler
    {
        private static string BOOTM = "";
        public static void DELETEUFILEEXTS(string fix)
        {
            // Extract the username from the fix string using regex
            string user = Regex.Replace(fix, @"(?i)HKU\\(.+?)\\.+\\FileExts\\.exe:.*", "$1");


            // Reload the user profile if needed (this depends on your application, implementation may vary)
            RegistryUtils.RELOAD(user);

            // Build the registry key to delete
            string key = @"HKU\" + user + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.exe";

            // Delete the registry value
            RegistryValueHandler.DeleteRegistryValue(key, "");

            // Unload the user profile if needed (this depends on your application, implementation may vary)
            RegistryUtils.REUNLOAD(user);
        }

        public void SETDEFBAT()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\.bat", "", "batfile", RegistryValueKind.String);
        }

        public void SETDEFCMD()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\.cmd", "", "cmdfile", RegistryValueKind.String);
        }

        public void SETDEFCOM()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\.com", "", "comfile", RegistryValueKind.String);
        }

        public void SETDEFCOMBAT()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\batfile\shell\open\command", "", "\"%1\" %*", RegistryValueKind.String);
        }

        public void SETDEFCOMCMD()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\cmdfile\shell\open\command", "", "\"%1\" %*", RegistryValueKind.String);
        }

        public void SETDEFCOMCOM()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\comfile\shell\open\command", "", "\"%1\" %*", RegistryValueKind.String);
        }

        public void SETDEFCOMEXE()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\exefile\shell\open\command", "", "\"%1\" %*", RegistryValueKind.String);
        }

        public void SETDEFCOMREG()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\regfile\shell\open\command", "", "regedit.exe \"%1\"", RegistryValueKind.String);
        }

        public void SETDEFCOMSCR()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\scrfile\shell\open\command", "", "\"%1\" /S", RegistryValueKind.String);
        }

        public void SETDEFEXE()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\.exe", "", "exefile", RegistryValueKind.String);
        }
        public void SETDEFICON()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\exefile\DefaultIcon", "", "%1", RegistryValueKind.String);
        }

        public void SETDEFICONBAT()
        {
            string value = @"%SystemRoot%\System32\imageres.dll,-68";  // Default value for newer OS versions
            if (Environment.OSVersion.Version < new System.Version(6, 0))  // Check if the OS is below Windows 6 (XP or Server 2003)
            {
                value = @"%SystemRoot%\System32\shell32.dll,-153";  // Legacy icon path for older OS versions
            }
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\batfile\DefaultIcon", "", value, RegistryValueKind.ExpandString);
        }

        public void SETDEFICONCMD()
        {
            string value = @"%SystemRoot%\System32\imageres.dll,-68";  // Default value for newer OS versions
            if (Environment.OSVersion.Version < new System.Version(6, 1))  // Check if the OS is below Windows 6.1 (Windows 7 or Server 2008)
            {
                value = @"%SystemRoot%\System32\shell32.dll,-153";  // Legacy icon path for older OS versions
            }
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\cmdfile\DefaultIcon", "", value, RegistryValueKind.ExpandString);
        }

        public void SETDEFICONCOM()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\comfile\DefaultIcon", "", @"%SystemRoot%\System32\shell32.dll,2", RegistryValueKind.ExpandString);
        }

        public void SETDEFICONREG()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\regfile\DefaultIcon", "", @"%SystemRoot%\regedit.exe,1", RegistryValueKind.ExpandString);
        }

        public void SETDEFLOGONUI()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\CLSID\{7986d495-ce42-4926-8afc-26dfa299cadb}\InprocServer32", "", @"%SystemRoot%\system32\authui.dll", RegistryValueKind.ExpandString);
        }

        public void SETDEFREG()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\.reg", "", "regfile", RegistryValueKind.String);
        }

        public void SETDEFSCR()
        {
            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\.scr", "", "scrfile", RegistryValueKind.String);
        }
        public void SETDEFWBEM()
        {
            if (BOOTM != "Recovery")
            {
                ProcessFix.KILLDLL();
            }

            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\CLSID\{F3130CDB-AA52-4C3A-AB32-85FFC23AF9C1}\InprocServer32", "", @"%systemroot%\system32\wbem\wbemess.dll", RegistryValueKind.ExpandString);
        }

        public void SETDEFWBEM7F()
        {
            if (BOOTM != "Recovery")
            {
                ProcessFix.KILLDLL();
            }

            RegistryValueHandler.RestoreRegistryValue(RegistryHive.LocalMachine, @"HKLM\Software\Classes\CLSID\{5839FCA9-774D-42A1-ACDA-D6A79037F57F}\InprocServer32", "", @"%systemroot%\system32\wbem\fastprox.dll", RegistryValueKind.ExpandString);
        }
        public static void DeleteFileExts(string software)
        {
            // Define the registry key path
            string keyExe = @"\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.exe";
            string key = $"HKLM\\{software}{keyExe}";

            // Delete the registry key
            RegistryKeyHandler.DeleteRegistryKey(key);
        }
    }
}

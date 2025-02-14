using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

namespace Wildlands_System_Scanner.Scripting.RegistryFix
{
    public class MSConfigFix
    {
        public static void MSCONFIGFIX(ref string fix)
        {
            try
            {
                if (Regex.IsMatch(fix, "(?i)startupreg|Services"))
                {
                    string keyVal = Regex.Replace(fix, ".+?: (.+) =>.+", "$1");
                    string key = $@"HKLM\SOFTWARE\Microsoft\Shared Tools\MSConfig\startupreg\{keyVal}";

                    if (fix.Contains("Services"))
                    {
                        key = $@"HKLM\SOFTWARE\Microsoft\Shared Tools\MSConfig\Services\{keyVal}";
                        RegistryKeyHandler.DeleteRegistryKey($@"HKLM\System\CurrentControlSet\Services\{keyVal}");
                    }

                    RegistryKeyHandler.DeleteRegistryKey(key);
                }

                if (fix.Contains("MSCONFIG\\startupfolder"))
                {
                    string keyPath = Regex.Replace(fix, ".+?: (.+) =>.+", "$1");
                    string key = $@"HKLM\SOFTWARE\Microsoft\Shared Tools\MSConfig\startupfolder\{keyPath}";
                    RegistryKeyHandler.DeleteRegistryKey(key);

                    string backPath = Regex.Replace(fix, ".+?=> (.+)", "$1");
                    FileFix.MoveFileNormal(backPath);
                }

                if (Regex.IsMatch(fix, "(?i)HKLM\\\\.+StartupApproved\\\\Run:"))
                {
                    string val1 = Regex.Replace(fix, ".+=> \"(.+)\"", "$1");
                    RegistryValueHandler.DeleteRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", val1);
                    RegistryValueHandler.DeleteRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", val1);
                }

                if (Regex.IsMatch(fix, "(?i)HKU.+StartupApproved\\\\Run:"))
                {
                    string val1 = Regex.Replace(fix, ".+=> \"(.+)\"", "$1");
                    string user = Regex.Replace(fix, "(?i)HKU\\\\(.+?)\\\\.+", "$1");

                    RegistryValueHandler.DeleteRegistryValue($@"HKU\{user}\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", val1);
                    RegistryValueHandler.DeleteRegistryValue($@"HKU\{user}\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", val1);
                }

                if (Regex.IsMatch(fix, "(?i)HKU\\\\.+StartupApproved\\\\StartupFolder:"))
                {
                    string user = Regex.Replace(fix, "(?i)HKU\\\\(.+?)\\\\.+", "$1");
                    string val1 = Regex.Replace(fix, ".+=> \"(.+)\"", "$1");

                    FileFix.MoveFileNormal(Path.Combine(FolderConstants.Startup, val1));
                    RegistryValueHandler.DeleteRegistryValue($@"HKU\{user}\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder", val1);
                }

                if (Regex.IsMatch(fix, "(?i)HKLM\\\\.+StartupApproved\\\\StartupFolder:"))
                {
                    string val1 = Regex.Replace(fix, ".+=> \"(.+)\"", "$1");
                    FileFix.MoveFileNormal(Path.Combine(FolderConstants.StartupCommon, val1));
                    RegistryValueHandler.DeleteRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder", val1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MSCONFIGFIX: {ex.Message}");
            }
        }
    }
}

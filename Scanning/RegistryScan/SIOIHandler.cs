using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;

//Needs Work


namespace Wildlands_System_Scanner
{
    public class SIOIHandler
    {
        public void SIOI(string hive, string key)
        {
            string clsid, file, company = "", cdate = "";
            int i = 0;
            string subKey;

            // Open registry key
            using (RegistryKey hKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, true))
            {
                if (hKey == null) return;

                while (true)
                {
                    subKey = RegistrySubKeyHandler.GetSubKey(hKey, i); // Assuming GetSubKey is a method to enumerate subkeys
                    if (subKey == null) break;

                    clsid = RegistryValueHandler.ReadRegistryValue(key + "\\" + subKey, "");
                    if (Regex.IsMatch(clsid, @"\{.+\}"))
                    {
                        string key1 = @"HKCR\CLSID\" + clsid + @"\InprocServer32";
                        file = RegistryValueHandler.ReadRegistryValue(key1, "");

                        if (file.Contains("mscoree.dll"))
                        {
                            MSCOREEHandler.MSCOREE(key1, ref file);

                        }

                        FileUtils.AAAAFP(); // Placeholder method for further operations

                        if (!File.Exists(file))
                        {
                            file += " -> " + "REGIST8"; // Placeholder for REGIST8
                        }
                        else
                        {
                            cdate = " [" + cdate + "]";
                        }

                        if (Regex.IsMatch(file, @"(?i)WINDOWS\\system32\\(hvsiofficeiconoverlayshellextension|EhStorShell|cscui|syncui|shell32|ntshrui|WorkfoldersShell|appresolver)\.dll") &&
                            Regex.IsMatch(company, @"(?i)Microsoft Corporation"))
                        {
                            // Do nothing or handle specific case
                        }
                        else if (Regex.IsMatch(file, @"(?i)\\Program Files\\Windows (Defender|Sidebar)\\(shellext|sbdrop)\.dll") &&
                                 Regex.IsMatch(company, @"(?i)Microsoft Corporation"))
                        {
                            // Do nothing or handle specific case
                        }
                        else if (Regex.IsMatch(file, @"(?i)\\Microsoft\\(SkyDrive|OneDrive)\\.+\\(SkyDriveShell|FileSyncShell)\.dll") &&
                                 Regex.IsMatch(company, @"(?i)Microsoft Corporation"))
                        {
                            // Do nothing or handle specific case
                        }
                        else if (Regex.IsMatch(file, @"(?i)\\Microsoft Office\\.+?\\(GrooveShellExtensions|GROOVEEX).dll") &&
                                 Regex.IsMatch(company, @"(?i)Microsoft Corporation"))
                        {
                            // Do nothing or handle specific case
                        }
                        else
                        {
                            // Add to array or log details
                            string[] existingArray = new string[0]; // Initialize an empty array

                            string logMessage = $"{hive}: [{subKey}] -> {clsid} => {file} {cdate} {company}";
                            Logger.Instance.LogPrimary(logMessage);

                            ;
                        }
                    }
                    i++;
                }
            }
        }
    }
}

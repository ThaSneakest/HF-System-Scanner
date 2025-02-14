using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Scripting;

namespace Wildlands_System_Scanner
{
    public class WMIHandler
    {

        static string ERDEL = "Error deleting";
        public static void WMI_HJK(string path, ref List<string> arr)
        {
            string[] classes = {
            "__FilterToConsumerBinding",
            "__TimerInstruction",
            "__AbsoluteTimerInstruction",
            "__IntervalTimerInstruction",
            "__EventFilter",
            "NTEventLogEventConsumer",
            "ActiveScriptEventConsumer",
            "CommandLineEventConsumer",
            "LogFileEventConsumer",
            "SMTPEventConsumer"
        };

            try
            {
                string scope = @"\\.\root\" + path;
                ManagementScope managementScope = new ManagementScope(scope);
                managementScope.Connect();

                foreach (string className in classes)
                {
                    if (path == "CIMV2" && Array.IndexOf(classes, className) > 5) continue;

                    ObjectQuery query = new ObjectQuery($"Select * from {className}");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, query);
                    ManagementObjectCollection queryCollection = searcher.Get();

                    foreach (ManagementObject obj in queryCollection)
                    {
                        string flag = "";
                        string prop1 = "";
                        string prop2 = "";
                        string prop3 = "";
                        string name = "";

                        if (Array.IndexOf(classes, className) == 0)
                        {
                            name = obj["Consumer"]?.ToString();
                            string filter = obj["Filter"]?.ToString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                name = $"{name}\",Filter=\"{filter}";
                                name = Regex.Replace(name, "Name=", "Name=\\");
                                name = Regex.Replace(name, "\"\"", "\\\"\"");
                                name = Regex.Replace(name, "\"$", "\\\"");
                            }
                        }
                        else if (Array.IndexOf(classes, className) == 1 || Array.IndexOf(classes, className) == 2 || Array.IndexOf(classes, className) == 3)
                        {
                            name = obj["TimerId"]?.ToString();
                        }
                        else
                        {
                            name = obj["Name"]?.ToString();
                        }

                        if (!string.IsNullOrEmpty(name))
                        {
                            switch (Array.IndexOf(classes, className))
                            {
                                case 4:
                                    prop1 = obj["Query"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop1)) prop1 = $"[Query => {prop1}]";
                                    break;
                                case 6:
                                    prop1 = obj["ScriptFileName"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop1)) prop1 = $"[ScriptFileName => {prop1}]";
                                    prop2 = obj["ScriptText"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop2) && prop2.Length > 300)
                                    {
                                        prop2 = $"{prop2.Substring(0, 300)} ({prop2.Length} chars).";
                                    }
                                    if (!string.IsNullOrEmpty(prop2)) prop2 = $"[ScriptText => {prop2}]";
                                    break;
                                case 7:
                                    prop1 = obj["CommandLineTemplate"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop1) && prop1.Length > 300)
                                    {
                                        prop1 = $"{prop1.Substring(0, 300)} ({prop1.Length} chars).";
                                    }
                                    if (!string.IsNullOrEmpty(prop1)) prop1 = $"[CommandLineTemplate => {prop1}]";
                                    prop2 = obj["ExecutablePath"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop2)) prop2 = $"[ExecutablePath => {prop2}]";
                                    prop3 = obj["WorkingDirectory"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop3)) prop3 = $"[WorkingDirectory => {prop3}]";
                                    break;
                                case 8:
                                    prop1 = obj["Filename"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop1)) prop1 = $"[Filename => {prop1}]";
                                    prop2 = obj["Text"]?.ToString();
                                    if (!string.IsNullOrEmpty(prop2)) prop2 = $"[Text => {prop2}]";
                                    break;
                            }

                            if (Regex.IsMatch(name, "(?i)ASEC|fuck|Powershell|sethomePage|Windows Events|coronav|SCM Event\\d+ Log Consumer|SCM Event\\d+ Log Filter|systemcore_Updater\\d+"))
                            {
                                flag = " <==== UPDATED";
                            }
                            else
                            {
                                if (path == "subscription")
                                {
                                    if (className == "__EventFilter" && Regex.IsMatch(name, "(?i)SCM Event Log Filter|Dell(Command|)PowerManager(Alert|PolicyChange|PowerPlanChange|PowerPlanSettingChange|PowerStateChange|UserLogin)"))
                                        continue;
                                    if (className == "NTEventLogEventConsumer" && name == "SCM Event Log Consumer") continue;
                                    if (className == "ActiveScriptEventConsumer" &&
                                        Regex.IsMatch(name, "(?i)Dell(Command|)PowerManager(Alert|PolicyChange|PowerPlanChange|PowerPlanSettingChange|PowerStateChange|UserLogin)"))
                                        continue;
                                    if (className == "__FilterToConsumerBinding" && Regex.IsMatch(name, "(?i)NTEventLogEventConsumer.Name=\\\"SCM Event Log Consumer\\\"|ActiveScriptEventConsumer.Name=\\\"Dell(Command|)PowerManager(Alert|PolicyChange|PowerPlanChange|PowerPlanSettingChange|PowerStateChange|UserLogin)"))
                                        continue;
                                }
                            }

                            arr.Add($"WMI:{path}\\{className}->{name}::{prop1}{prop2}{prop3}{flag}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static void WMI_HJKFIX()
        {
            WMI_HJKFIXIT();
            string COMERR = "";
        }

        public static void WMI_HJKFIXIT()
        {
            string OBJWMISERVICE;
            ManagementObjectCollection DEVCOLITEMS;
            ManagementObject OBJECT;
            string NAMESPACE = Regex.Replace("WMI_FIX_PLACEHOLDER", @"WMI:(.+)\\.+->.+::.*", "$1");
            string ACLASS = Regex.Replace("WMI_FIX_PLACEHOLDER", @"WMI:.+\\(.+?)->.+::.*", "$1");
            string NAMEE = Regex.Replace("WMI_FIX_PLACEHOLDER", @"WMI:.+->(.+?)::.*", "$1");

            // Initialize the WMI Service (C# equivalent of ObjGet)
            string query = @"\\.\root\" + NAMESPACE;

            try
            {
                ManagementScope scope = new ManagementScope(query);
                scope.Connect();
                ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM " + ACLASS);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, objectQuery);
                DEVCOLITEMS = searcher.Get();

                string EXIST = "";
                foreach (ManagementObject obj in DEVCOLITEMS)
                {
                    string NAME = "";
                    string FILTER = "";
                    if (ACLASS == "__FilterToConsumerBinding")
                    {
                        NAME = obj["Consumer"].ToString();
                        FILTER = obj["Filter"].ToString();
                        if (!string.IsNullOrEmpty(NAME))
                        {
                            NAME += "\",Filter=\"" + FILTER;
                            NAME = Regex.Replace(NAME, "Name=", "Name=\\");
                            NAME = Regex.Replace(NAME, "\"\"", "\\\"\"");
                            NAME = Regex.Replace(NAME, "\"$", "\\\"");
                        }
                    }
                    else if (Regex.IsMatch(ACLASS, "(?i)__TimerInstruction|__AbsoluteTimerInstruction|__IntervalTimerInstruction"))
                    {
                        NAME = obj["TimerId"].ToString();
                    }
                    else
                    {
                        NAME = obj["Name"].ToString();
                    }

                    if (NAME == NAMEE) EXIST = "1";
                }

                if (string.IsNullOrEmpty(EXIST))
                {
                 //   Logger.NFOUND(NAMEE);
                }
                else
                {
                    string DEL = "";
                    if (ACLASS == "__FilterToConsumerBinding")
                    {
                        DEL = "__FilterToConsumerBinding.Consumer=\"" + NAMEE + "\"";
                    }
                    else if (Regex.IsMatch(ACLASS, "(?i)__TimerInstruction|__AbsoluteTimerInstruction|__IntervalTimerInstruction"))
                    {
                        DEL = ACLASS + ".TimerId=\"" + NAMEE + "\"";
                    }
                    else
                    {
                        DEL = ACLASS + ".Name=\"" + NAMEE + "\"";
                    }

                    DEL = Regex.Replace(DEL, @"\\(?!"")", @"\\\\");

                    // C# equivalent of Delete functionality (WMI delete)
                    ManagementObject DELETE_OBJ = new ManagementObject(scope.Path.Path + "\\" + DEL);
                    DELETE_OBJ.Delete();

                    if (DELETE_OBJ != null)
                    {
                      //  Logger.Deleted(NAMEE);
                    }
                    else
                    {
                    //    Logger.WriteToLog(Logger.WildlandsLogFile, NAMEE + " => " + ERDEL + ". Error: " + "Error code" + "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
              //  Logger.AddToLog(NAMEE + " => " + ERDEL + ". ErrorCode: " + ex.Message + "\r\n");

            }
        }

        public static void WMIPRVSE()
        {
            string BOOTM = "Non-Recovery"; // This is a placeholder; change accordingly
            string SYSTEMROOT;
            string FIX = "[Default]"; // Placeholder for the FIX value
            string SOFTWARE = "SOFTWARE"; // Placeholder for the SOFTWARE value

            if (BOOTM != "Recovery")
            {
                ProcessFix.KILLDLL();
            }

            // Read SystemRoot registry value
            SYSTEMROOT = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\" + SOFTWARE + @"\Microsoft\Windows NT\CurrentVersion", "SystemRoot", null);

            if (FIX.Contains("[Default]"))
            {
                // Restore registry value
                RegistryValueHandler.RestoreRegistryValue(
                    RegistryHive.LocalMachine,
                    SOFTWARE + @"\Classes\CLSID\{73E709EA-5D93-4B2E-BBB0-99B7938DA9E4}\LocalServer32",
                    "",
                    SYSTEMROOT + @"\system32\wbem\wmiprvse.exe",
                    RegistryValueKind.String
                );

            }

            if (FIX.Contains("[a]"))
            {
                // Delete registry value
                RegistryValueHandler.DeleteRegistryValue(@"HKEY_LOCAL_MACHINE\" + SOFTWARE + @"\Classes\CLSID\{73E709EA-5D93-4B2E-BBB0-99B7938DA9E4}\LocalServer32", "a");
            }
        }
    }
}

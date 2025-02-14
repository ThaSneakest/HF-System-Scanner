using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

//Tested and working

public class NetworkServiceHandler
{
    public static void NETSVC()
    {
        try
        {
            string[] netSvcArray;
            string serviceDll;

            // Define the base registry key
            string registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SvcHost";

            // Read the "netsvcs" value from the registry
            object netSvcValue = Registry.GetValue(registryKey, "netsvcs", null);
            if (netSvcValue == null)
            {
                Console.WriteLine("No 'netsvcs' value found in the registry.");
                return;
            }

            // Handle the case where the value is a multi-string (REG_MULTI_SZ)
            if (netSvcValue is string[])
            {
                netSvcArray = (string[])netSvcValue;
            }
            else if (netSvcValue is string)
            {
                netSvcArray = new[] { (string)netSvcValue };
            }
            else
            {
                Console.WriteLine("Unexpected data type for 'netsvcs' value.");
                return;
            }

            // Clean up the NETSVC array using regex patterns
            var cleanedNetSvcList = new List<string>();
            foreach (var svc in netSvcArray)
            {
                // Normalize line endings and remove unwanted entries
                string cleanedSvc = Regex.Replace(svc, @"([^\v]+)[\v]*", "$1");
                if (!Regex.IsMatch(cleanedSvc, @"(?i)(sacsvr|dcsvc|hns|HgClientService|nvagent|TroubleshootingSvc|WManSvc|LxpSvc|PushToInstall|InstallService|LxssManager|debugregsvc|6to4|AppMgmt|AudioSrv|Browser|CryptSvc|DMServer|DHCP|ERSvc|EventSystem|ezSharedSvc|FastUserSwitchingCompatibility|HidServ|Ias|Iprip|Irmon|LanmanServer|LanmanWorkstation|Messenger|Netman|Nla|Ntmssvc|NWCWorkstation|Nwsapagent|UxTuneUp|Rasauto|Rasman|Remoteaccess|Schedule|Seclogon|SENS|Sharedaccess|SRService|Tapisrv|Themes|TrkWks|W32Time|WZCSVC|Wmi|WmdmPmSp|winmgmt|wscsvc|xmlprov|BITS|wuauserv|ShellHWDetection|helpsvc|WmdmPmSN|napagent|hkmsvc|AeLookupSvc|CertPropSvc|SCPolicySvc|gpsvc|IKEEXT|TermService|LogonHours|PCAudit|uploadmgr|iphlpsvc|AppInfo|msiscsi|MMCSS|wercplsupport|EapHost|ProfSvc|SessionEnv|BDESVC|hkmsvc|NcaSvc|DsmSvc|SystemEventsBroker|wlidsvc|lfsvc|MsKeyboardFilter|DmEnrollmentSvc|dosvc|DcpSvc|UserManager|NetSetupSvc|RetailDemo|UsoSvc|dmwappushservice|WalletSvc|XblGameSave|XblAuthManager|XboxNetApiSvc|shpamsvc|wisvc|WpnService|NaturalAuthentication|xbgm|TokenBroker|XboxGipSvc|TokenBroker)"))
                {
                    cleanedNetSvcList.Add(cleanedSvc);
                }
            }

            {
                foreach (var netService in cleanedNetSvcList)
                {
                    string serviceKey = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\{netService}";

                    try
                    {
                        // Attempt to retrieve ServiceDLL value
                        serviceDll = (string)Registry.GetValue(serviceKey, "ServiceDLL", null);
                        if (string.IsNullOrEmpty(serviceDll))
                        {
                            serviceDll = (string)Registry.GetValue(serviceKey + @"\Parameters", "ServiceDLL", null);
                        }

                        if (string.IsNullOrEmpty(serviceDll))
                        {
                            Logger.Instance.LogPrimary($"NETSVC: {netService} -> No ServiceDLL found.");
                        }
                        else
                        {
                            // Normalize ServiceDLL path
                            serviceDll = Regex.Replace(serviceDll, @"(?i)%systemroot%", @"C:\Windows");

                            if (File.Exists(serviceDll))
                            {
                                var companyName = FileUtils.GetFileVersionInfo(serviceDll, "CompanyName");
                                Logger.Instance.LogPrimary($"NETSVC: {netService} -> {serviceDll} ({companyName})");
                            }
                            else
                            {
                                Logger.Instance.LogPrimary($"NETSVC: {netService} -> {serviceDll} (File not found)");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogPrimary($"NETSVC: {netService} -> Error: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in NETSVC: {ex.Message}");
        }
    }

}

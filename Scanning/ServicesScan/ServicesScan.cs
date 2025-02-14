using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scanning.ServicesScan
{
    public class ServicesScan
    {
        // Define a method to scan services and filter out whitelisted and flagged blacklisted services
        public void ScanServices(StreamWriter writer)
        {
            // Define the whitelist of services to exclude from logging (in lowercase)
            HashSet<string> servicesWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "aelookupsvc",  // Add more services to exclude here, in lowercase
                "AJRouter",
                "alg",
                "appidsvc",
                "appinfo",
                "appmgmt",
                "appreadiness",
                "appvclient",
                "appxsvc",
                "assignedaccessmanagersvc",
                "audioendpointbuilder",
                "audiosrv",
                "autotimesvc",
                "axinstsv",
                "bdesvc",
                "bfe",
                "bits",
                "brokerinfrastructure",
                "btagservice",
                "bthavctpsvc",
                "bthserv",
                "camsvc",
                "cdpsvc",
                "certpropsvc",
                "clicktorunsvc",
                "clipsvc",
                "cloudidsvc",
                "comsysapp",
                "coremessagingregistrar",
                "cryptsvc",
                "cscservice",
                "dcomlaunch",
                "dcsvc",
                "defragsvc",
                "deviceassociationservice",
                "deviceinstall",
                "devquerybroker",
                "dhcp",
                "diagnosticshub.standardcollector.service",
                "diagsvc",
                "diagtrack",
                "dialogblockingservice",
                "dispbrokerdesktopsvc",
                "displayenhancementservice",
                "dmenrollmentsvc",
                "dmwappushservice",
                "dnscache",
                "dosvc",
                "dot3svc",
                "dps",
                "dsmsvc",
                "dssvc",
                "dusmsvc",
                "eaphost",
                "efs",
                "embeddedmode",
                "entappsvc",
                "eventlog",
                "eventsystem",
                "fax",
                "fdphost",
                "fdrespub",
                "fhsvc",
                "fontcache",
                "fontcache3.0.0.0",
                "frameserver",
                "gameinput service",
                "gameinputsvc",
                "gamingservices",
                "gamingservicesnet",
                "gpsvc",
                "graphicsperfsvc",
                "hgclientservice",
                "hidserv",
                "hns",
                "hvhost",
                "icssvc",
                "ikeext",
                "installservice",
                "iphlpsvc",
                "ipxlatcfgsvc",
                "jhi_service",
                "keyiso",
                "ktmrm",
                "lanmanserver",
                "lanmanworkstation",
                "lfsvc",
                "licensemanager",
                "lltdsvc",
                "lmhosts",
                "lsm",
                "lxpsvc",
                "mapsbroker",
                "mcpmanagementservice",
                "mdcoresvc",
                "mixedrealityopenxrsvc",
                "mpssvc",
                "msdtc",
                "msiscsi",
                "msiserver",
                "mskeyboardfilter",
                "naturalauthentication",
                "ncasvc",
                "ncbservice",
                "ncdautosetup",
                "netlogon",
                "netman",
                "netprofm",
                "netsetupsvc",
                "nettcpportsharing",
                "ngcctnrsvc",
                "ngcsvc",
                "nlasvc",
                "nsi",
                "nvagent",
                "ose64",
                "p2pimsvc",
                "p2psvc",
                "pcasvc",
                "peerdistsvc",
                "perceptionsimulation",
                "perfhost",
                "phonesvc",
                "pla",
                "plugplay",
                "pnrpautoreg",
                "pnrpsvc",
                "policyagent",
                "power",
                "printnotify",
                "profsvc",
                "pushtoinstall",
                "qwave",
                "rasauto",
                "rasman",
                "remoteaccess",
                "remoteregistry",
                "retaildemo",
                "rmsvc",
                "rpceptmapper",
                "rpclocator",
                "rpcss",
                "samss",
                "scardsvr",
                "scdeviceenum",
                "schedule",
                "scpolicysvc",
                "sdrsvc",
                "seclogon",
                "securityhealthservice",
                "semgrsvc",
                "sens",
                "sense",
                "sensordataservice",
                "sensorservice",
                "sensrsvc",
                "sessionenv",
                "sgrmbroker",
                "sharedaccess",
                "sharedrealitysvc",
                "shellhwdetection",
                "shpamsvc",
                "smphost",
                "smsrouter",
                "snmptrap",
                "spectrum",
                "spooler",
                "sppsvc",
                "sqlwriter",
                "ssdpsrv",
                "ssh-agent",
                "sstpsvc",
                "staterepository",
                "stisvc",
                "storsvc",
                "svsvc",
                "swprv",
                "sysmain",
                "systemeventsbroker",
                "tabletinputservice",
                "tapisrv",
                "termservice",
                "themes",
                "tieringengineservice",
                "timebrokersvc",
                "tokenbroker",
                "trkwks",
                "troubleshootingsvc",
                "trustedinstaller",
                "tzautoupdate",
                "uevagentservice",
                "uhssvc",
                "umrdpservice",
                "upnphost",
                "usermanager",
                "usosvc",
                "vacsvc",
                "vaultsvc",
                "vds",
                "vmcompute",
                "vmicguestinterface",
                "vmicheartbeat",
                "vmickvpexchange",
                "vmicrdv",
                "vmicshutdown",
                "vmictimesync",
                "vmicvmsession",
                "vmicvss",
                "vmms",
                "vsinstallerelevationservice",
                "vss",
                "vsstandardcollectorservice150",
                "w32time",
                "waasmedicsvc",
                "walletservice",
                "warpjitsvc",
                "wbengine",
                "wbiosrvc",
                "wcmsvc",
                "wcncsvc",
                "wdiservicehost",
                "wdisystemhost",
                "wdnissvc",
                "webclient",
                "wecsvc",
                "wephostsvc",
                "wercplsupport",
                "wersvc",
                "wfdsconmgrsvc",
                "wiarpc",
                "windefend",
                "winhttpautoproxysvc",
                "winmgmt",
                "winrm",
                "wisvc",
                "wlansvc",
                "wlidsvc",
                "wlpasvc",
                "wmansvc",
                "wmiapsrv",
                "wmiregistrationservice",
                "wmpnetworksvc",
                "workfolderssvc",
                "wpcmonsvc",
                "wpdbusenum",
                "wpnservice",
                "wscsvc",
                "wsearch",
                "wuauserv",
                "wwansvc",
                "xblauthmanager",
                "xblgamesave",
                "xboxgipsvc",
                "xboxnetapisvc",
                "aarsvc_9c69c",
                "bcastdvruserservice_9c69c",
                "bluetoothuserservice_9c69c",
                "captureservice_9c69c",
                "cbdhsvc_9c69c",
                "cdpusersvc_9c69c",
                "consentuxusersvc_9c69c",
                "credentialenrollmentmanagerusersvc_9c69c",
                "deviceassociationbrokersvc_9c69c",
                "devicepickerusersvc_9c69c",
                "devicesflowusersvc_9c69c",
                "messagingservice_9c69c",
                "onesyncsvc_9c69c",
                "pimindexmaintenancesvc_9c69c",
                "printworkflowusersvc_9c69c",
                "udkusersvc_9c69c",
                "unistoresvc_9c69c",
                "userdatasvc_9c69c",
                "wpnuserservice_9c69c",
                "aarsvc_1eb9739",
                "bcastdvruserservice_1eb9739",
                "bluetoothuserservice_1eb9739",
                "captureservice_1eb9739",
                "cbdhsvc_1eb9739",
                "cdpusersvc_1eb9739",
                "consentuxusersvc_1eb9739",
                "credentialenrollmentmanagerusersvc_1eb9739",
                "deviceassociationbrokersvc_1eb9739",
                "devicepickerusersvc_1eb9739",
                "devicesflowusersvc_1eb9739",
                "messagingservice_1eb9739",
                "onesyncsvc_1eb9739",
                "pimindexmaintenancesvc_1eb9739",
                "printworkflowusersvc_1eb9739",
                "udkusersvc_1eb9739",
                "unistoresvc_1eb9739",
                "userdatasvc_1eb9739",
                "wpnuserservice_1eb9739"

            };

            // Define the blacklist of services to flag as "Potential Malicious Service" (in lowercase)
            HashSet<string> servicesBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "service kmseldi",  // Example malicious service (replace with actual)
                "maliciousservice2",  // Example malicious service (replace with actual)
            };

            StringBuilder services = new StringBuilder();

            try
            {
                // Query the services using WMI
                ManagementObjectCollection serviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_Service").Get();

                foreach (ManagementObject service in serviceCollection)
                {
                    try
                    {
                        // Retrieve the service name
                        string serviceName = service["Name"]?.ToString()?.Trim().ToLower();  // Trim and convert to lowercase
                        string pathName = service["PathName"] != null ? service["PathName"].ToString()?.Trim() : string.Empty;
                        string cleanedPathName = pathName.ToLower().Replace("\"", "");

                        // Debugging output to verify the names
                        Console.WriteLine($"Service Name: {serviceName}, Path: {cleanedPathName}");

                        // Check if the service is in the whitelist
                        if (!servicesWhitelist.Contains(serviceName))
                        {
                            // If the service is in the blacklist, mark it as "Potential Malicious Service"
                            if (servicesBlacklist.Contains(serviceName))
                            {
                                services.AppendLine($"{serviceName} - {cleanedPathName} --> Potential Malicious Service");
                            }
                            else
                            {
                                services.AppendLine($"{serviceName} - {cleanedPathName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exception if necessary
                        Console.WriteLine($"Error processing service: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if necessary
                Console.WriteLine($"Error querying services: {ex.Message}");
            }

            // Write the resulting list of services to the StreamWriter
            writer.WriteLine(services.ToString());
        }
    }
}

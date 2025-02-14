using Microsoft.Win32;
using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Registry;
using Wildlands_System_Scanner.Utilities;

public class FirewallHandler
{

    public static void ProcessFirewallRules(string domain, bool isAuthorizedApplications = false)
    {
        string subKey = isAuthorizedApplications ? @"\AuthorizedApplications" : @"\GloballyOpenPorts";
        string registryPath = $@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\\{domain}{subKey}\List";

        // Change the variable type to List<KeyValuePair<string, string>>
        List<KeyValuePair<string, string>> entries = RegistryValueHandler.GetRegistryValuesAsList(registryPath);

        if (entries == null || entries.Count == 0) return;

        foreach (var entry in entries)
        {
            string valueName = entry.Key;
            string valueData = entry.Value;

            // Filter out certain system paths
            if (!Regex.IsMatch(valueName, @"(?i)%windir%\\(system32|Network Diagnostic)\\(sessmgr|xpnetdiag)\.exe"))
            {
                string sanitizedValueName = Regex.Replace(valueName, @"\\", @"\\");
                sanitizedValueName = Regex.Replace(sanitizedValueName, @"(\(|\))", @"\$1");
                valueData = Regex.Replace(valueData, sanitizedValueName, "");
                valueData = Regex.Replace(valueData, @"^:\*:", "");

                Console.WriteLine($"{domain}{subKey}: [{valueName}] => {valueData}");
            }
        }
    }



    public static List<string> ListNTQueryStreams(string filePath)
    {
        IntPtr hFile = Kernel32NativeMethods.CreateFile(
            filePath,
            0x80000000, // GENERIC_READ
            0x00000001, // FILE_SHARE_READ
            IntPtr.Zero,
            0x3,        // OPEN_EXISTING
            0x80,       // FILE_ATTRIBUTE_NORMAL
            IntPtr.Zero);
        if (hFile == IntPtr.Zero) return new List<string>();

        List<string> streams = new List<string>();
        int allocationSize = 512;
        var ioStatusBlock = new Structs.IO_STATUS_BLOCK();
        IntPtr buffer;

        do
        {
            buffer = Marshal.AllocHGlobal(allocationSize);
            int status = NtdllNativeMethods.NtQueryInformationFile(hFile, ref ioStatusBlock, buffer, allocationSize, 22);

            if (status == 0 && ioStatusBlock.Information != 0)
            {
                streams = ParseStreamInformation(buffer);
                break;
            }

            allocationSize *= 2;
            Marshal.FreeHGlobal(buffer);

        } while (allocationSize <= 262144);

        Kernel32NativeMethods.CloseHandle(hFile);
        return streams;
    }
    private static List<string> ParseStreamInformation(IntPtr buffer)
    {
        List<string> streams = new List<string>();
        IntPtr currentPtr = buffer;

        while (true)
        {
            var fileStreamInfo = Marshal.PtrToStructure<Structs.FILE_STREAM_INFORMATION>(currentPtr);

            // Process the stream if StreamNameLength is greater than 14
            if (fileStreamInfo.StreamNameLength > 14)
            {
                string streamName = Marshal.PtrToStringUni(
                    IntPtr.Add(currentPtr, Marshal.SizeOf<Structs.FILE_STREAM_INFORMATION>()),
                    (int)(fileStreamInfo.StreamNameLength / 2));  // Casting to int here since it should be safe now
                streams.Add(Regex.Replace(streamName, "(.*):.*", "$1"));
            }

            // Exit loop if NextEntryOffset is 0
            if (fileStreamInfo.NextEntryOffset == 0) break;

            // Safely cast NextEntryOffset to long and check range
            ulong nextEntryOffset = fileStreamInfo.NextEntryOffset;

            // Ensure the ulong value fits into the IntPtr offset range
            if (nextEntryOffset > (ulong)Int32.MaxValue)
            {
                throw new InvalidOperationException($"NextEntryOffset value {nextEntryOffset} exceeds the allowed range for an int.");
            }

            // Convert NextEntryOffset to int and move the pointer
            currentPtr = IntPtr.Add(currentPtr, (int)nextEntryOffset);
        }

        return streams;
    }
    

    public static void CHECKFW(string SRV)
    {
        // Simulate _SRVSTAT (service status check)
        string serviceStatus = _SRVSTAT(SRV);
        if (SystemConstants.BootMode == "Normal" && serviceStatus != "R")
        {
          //  Logger.FileWrite(HADDITION, SRV + " => " + FWNRUN);
        }
    }

    public static void CHECKFWRUN()
    {
        // Check if system is in "Safe Mode (minimal)"
        if (SystemConstants.BootMode == "Safe Mode (minimal)") return;

        try
        {
            // Creating HNetCfg.FwMgr COM object for firewall configuration
            dynamic fwMgr = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr"));
            dynamic profile = fwMgr.LocalPolicy.CurrentProfile;

            // Checking if firewall is enabled or not
            if (profile.FirewallEnabled)
            {
             //  Logger.FileWrite(HADDITION, WINFW + " " + INTERNET2);
            }
            else
            {
             //   Logger.FileWrite(HADDITION, WINFW + " " + FWDIS);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error accessing firewall: " + ex.Message);
        }
    }

    // Simulating service status check (like _SRVSTAT)
    private static string _SRVSTAT(string service)
    {
        // This is a mock implementation for the service status
        return service == "some_service" ? "Running" : "Stopped";
    }


    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "FRST", "logs");
    private static readonly string FirewallRulesKey = @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules";

    public static void CheckFirewallRules()
    {
        Console.WriteLine("Scanning Firewall Rules...");
        string logPath = Path.Combine(LogsDirectory, "FirewallRules.log");

        // Initialize log file
        using (StreamWriter logWriter = new StreamWriter(logPath, true))
        {
            logWriter.WriteLine();
            logWriter.WriteLine("==================== Firewall Rules (Whitelisted) ================");
            logWriter.WriteLine();

            // Open Registry Key
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(FirewallRulesKey))
            {
                if (key == null)
                {
                    Console.WriteLine("Failed to access firewall rules registry key.");
                    return;
                }

                // Iterate over firewall rules
                foreach (string valueName in key.GetValueNames())
                {
                    string valueData = key.GetValue(valueName)?.ToString();
                    if (string.IsNullOrEmpty(valueData))
                        continue;

                    // Skip certain predefined rules
                    if (Regex.IsMatch(valueData, @"(?i)Action=Allow.+(ehome|System32(\\wbem|))\\(mmc|snmp|tlntsvr|vmms|nfsclnt|deviceenroller|mqsvc|omadmclient|dmcertinst|sppextcomobj|svchost|wudfhost|netproj|mcrmgr|mcx2prov|ehshell|msra|raserver|msdtc|services|unsecapp|plasrv|lsass|p2phost|vdsldr|vds|spoolsv|snmptrap|wininit|proximityuxhost|dashost|NetEvtFwdr|RmtTpmVscMgrSvr|mdeserver|RdpSa|CastSrv)\.exe"))
                        continue;

                    if (Regex.IsMatch(valueData, @"(?i)Action=Allow.+%ProgramFiles%\\(Windows Media Player|Windows MultiPoint Server)\\(WmsDashboard|WmsManager|Wmssvc|wmplayer|wmpnetwk)\.exe"))
                        continue;

                    if (Regex.IsMatch(valueData, @"(?i)Action=Allow.+:\\Windows\\Microsoft.NET\\Framework\d+\\[^\\]+\\SMSvcHost.exe"))
                        continue;

                    if (Regex.IsMatch(valueData, @"(?i)Action=Allow.+:\\Program Files\\Microsoft Office\\Office\d+\\(outlook|GROOVE|ONENOTE)\.exe"))
                        continue;

                    if (Regex.IsMatch(valueData, @"(?i)Action=Allow.+:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe"))
                        continue;

                    string allowOrBlock = Regex.IsMatch(valueData, @"(?i)Action=Block") ? "(Block) " : "(Allow) ";

                    if (Regex.IsMatch(valueData, @"(?i)App=(?!System)"))
                    {
                        string appPath = Regex.Match(valueData, @"(?i).+app=(.+?)\|.+").Groups[1].Value;
                        string company = FileUtils.GetFileCompany(appPath);
                        logWriter.WriteLine($"FirewallRules: [{valueName}] => {allowOrBlock}{appPath} {company}");
                    }
                    else if (Regex.IsMatch(valueData, @"(?i)LPort=(?!RPC-EPMap)") && !valueData.Contains("App=System"))
                    {
                        string lPort = Regex.Match(valueData, @"(?i).+(LPort=.+?)\|.+").Groups[1].Value;
                        logWriter.WriteLine($"FirewallRules: [{valueName}] => {allowOrBlock}{lPort}");
                    }
                }
            }
        }

        CheckFirewallProfiles("DomainProfile");
        CheckFirewallProfiles("PublicProfile");
        CheckFirewallProfiles("StandardProfile");
    }


    // Placeholder for checking specific firewall profiles.
    private static void CheckFirewallProfiles(string profileName)
    {
        // Add specific logic for profiles if needed
        Console.WriteLine($"Checking {profileName}...");
    }

    public static List<string> ListFirewallRules()
    {
        List<string> firewallRules = new List<string>();

        // Create an instance of the firewall manager
        Type type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
        INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(type);

        // Get the list of firewall rules
        foreach (INetFwRule rule in firewallPolicy.Rules)
        {
            firewallRules.Add($"Name: {rule.Name}, Enabled: {rule.Enabled}, Action: {rule.Action}, Direction: {rule.Direction}, Protocol: {rule.Protocol}");
        }

        return firewallRules;
    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner;

public class BlacklistSrv
{
    private static string REGEXPR;
    private static string UPD1 = "Attention";
    private static string NOACC = "No Access"; // Replace with actual value
    private static string BOOTM = "Normal"; // Replace with actual value
    private static string FILENS = "FileName"; // Replace with actual value
    private static string NO = "No"; // Replace with actual value

    public static void BLACKLISTSRV()
    {
        // Replace all occurrences of regex and append update string
        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+(?<!System32)\\\\csrss\\.exe[^<]+?)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);
        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][12345] (RDP-Controller|ScreenConnect.*|OutbyteDUHelper|WindowsUpdate|GoogleUpdateTaskMachineQC|compiler|Atruct.*|Registry1|Dinosecurity\\w+|game-downloader|secureboot|AltruisticsService|MsHelper|asrscan|WMS|MFService|Great Discover|RestoroActiveProtection|AppService\\w?|pubgame-updater|ZzNetSvc|WinDefender|SystemServices|RecipeHub_2jService|SysSvc|Kolnixo|WNetworkMgmt|SystemUpdate|MicroService|Winmon(|FS|ProcessMonito)|Quoteex|NativeDesktopMediaService|Ea3Host|ProgramData|chip1click|cstlsvc|iSafe.*?|mptpmdxm|JszipService|TMKernelHelpU|TMService|PrefersSecure|NetUtils2016srv|Subair|backlh|Nettrans|IISvr|SysLinkMapper|srcsrv|MVCSrv|AppleCloudSvc|WinAppSvr|MaohaWifiSvr|HPWombat Service|Web Cache Manager|CORE Software Updater|WindowsOfficeSrv|WeatherService|Disrupt Software Update|NetUtils\\d{4}|qdcomsvc|PremierOpinion|NetUtils\\d+|SPS|RunBooster|OtherSearch|WinDivert\\d+|Lace\\d+|iThemes5|windowsmanagementservice|Dataup|mwescontroller|mweshield.*|Undp\\d*|FastCompress|ProntSpooler|nrtService|netaie|confine.*|MFLService.*|consumerinput.*|(Service|Update) Mgr .+|VSSS|hola_(updater|svc)|Utatity|BitTorrent|BluetoothPoint|PCKeeperOcfService|PCKeeper2Service|AccountService|UniversalUpdater|AlaPerformance|.*Sale.?Charger|7a094844|WindowsMangerProtect|MediaUpdater.*|JokerAds.*|BeSecure.*|Bogard.*|mintcast.*|Shell&ServicesEngine.*|globalUpdate|globalUpdatem|MyOSProtect|Updater\\.exe|PrivoxyService|Live Malware Protection|Savdm|SavdmMonitor|StormAlert|sndappv2|Application Sendori|WindowsProtectManger|(Update|Util) Mega Browse|SecureUpdateSvc|RelevantKnowledge|winzipersvc|Adobe Licensing Console|SavingsbullFilterService64|desksvc|qtypesvc|LPTSystemUpdater|PCProtect|ProtectMonitor|Re-markit|TorchCrashHandler|Update EnhanceTronic|Update qualitink|Util EnhanceTronic|vosr|Wajam.+?|SoEasy.*);[^<]+?)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+(?<!\\\\amd64)\\\\nssm\\.exe[^<]+?|.+\\\\(?:Microsoft\\\\IObitUnlocker|ProgramData\\\\WindowsServices|registry\\\\reghost\\.exe|csrss\\.exe|ProgramData\\\\GoogleUP\\\\|Altruist\\\\|MaskVPN\\\\|Web Companion\\\\|runchos\\.exe|winnet\\.exe|winsvc\\.exe|Pictures\\\\Minor Policy|Reimage\\\\|ORBTR\\\\|transmission-qt\\.exe|VBoxNetFlt\\.exe|SearchProtect\\\\|ChromiumUpdate\\.exe|TotalAV\\\\|SAntivirus\\\\|Segurazo\\\\|MyPC Backup\\\\|bytefence\\\\|Program Files[^\\\\]*\\\\Mail\\.Ru\\\\|XBox\\\\|System\\.exe|ProgramData\\\\.+Provider\\.dll|UCBrowser\\\\|(Local|Windows)\\\\Temp(\\\\|/)|UBar\\\\|Program Files(| \\(x86\\))\\\\Firefox\\\\bin|amuleC\\d|WinArcher|ProxyGate|PC Speed Up|ShopperPro|YTDownloader|desktopfindkey|MSUser\\.Default|SoSoIm|SOEasy|Common Files\\\\.oobzo|PrefersSecure|Main Services\\\\|\\\\VLCStreamer\\\\)[^<]+?|(\\\\Windows|%SystemRoot%)\\\\servicing\\\\(?!TrustedInstaller)[^\\\\]+\\.exe[^<]+?|.+\\bpowershell\\b.+\\.ps1|.+\\bmshta\\b\\s+http[^<]+?|:\\\\Users\\\\[^\\\\]+\\\\[^\\\\]+\\.(vbs|bat|exe)\\b[^<]+?)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, @"(?i)([RSU][2345] (\w+) Updater; .:\\Program Files\\\2 Updater\\\2 Updater\.exe .+)(?i)\r\n", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (?:Diagnostics|Proxy);.+\\\\service\\.exe.*)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] scan;.+\\\\iYogi.+)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+\\\\ProgramData\\\\.+?\\[[^]]+\\] \\(\\) \\[[^]]+\\][^<]*)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+\\[[^]]+\\] \\[[^]]+\\][^<]*?)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+\\\\(?:ProwebiSvc|DeltaFix|OptProMon|decodit)\\.dll.*)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+(MPCProtectService|zdengine|\\\\VOsrv|WindowsLogger\\\\winlogger)\\.exe.*)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (.+?); .:\\Program Files\\\\\\2\\\\\\2\\.exe \\[.+?\\] \\(\\) \\[" + FILENS + "\\])\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+\\\\Program Files\\\\(LuDaShi|ziptool|ScreenshotPro|WebDiscoverBrowser|amuleC)\\\\.+(?:\\)|\\]))\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, @"([RSU][2345] [a-z0-9]{32,}; .:\\Program Files\\[a-z0-9]{32,}\\[a-z0-9]{32,}\.exe \[.+\] \(\) \[.+\])\x0B{2}", "$1 <==== " + UPD1 + Environment.NewLine);


        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] .+; [c-z]:\\\\ProgramData\\\\[^\\\\]+\\.(exe|dll)(| \\[.+\\] \\(.*\\)(| \\[.+\\])))\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] WindowsSecurity; [c-z]:\\\\ProgramData\\\\Windows Security\\\\winsecurity\\.exe \\[.+\\] \\(.+\\) \\[.+\\])\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345].+; [c-z]:\\\\Program Files\\\\XBox\\\\XBLive\\.exe \\[.+\\] \\(.+\\))\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345].+; .+(?<!system32|SysWOW64)\\\\svchost\\.exe \\[.+\\] \\(.*\\).*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345].+; [c-z]:\\\\Program Files\\\\Mozilla Firefox\\\\.+?\\.dll \\[.+\\] \\(.*\\) \\[.+\\])\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (.+); .+?\\\\AppData\\\\(Roaming|Local)\\\\\\2\\\\[^\\\\]+?\\.dll.*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (.+); .+?\\\\AppData\\\\(Roaming|Local)\\\\\\2\\\\\\2\\.exe .*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (.+); [c-z]:\\\\ProgramData\\\\\\2\\\\\\2\\.(dll|exe).*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (.+); [c-z]:\\\\ProgramData\\\\Microsoft\\\\(?!Windows Defender).+)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, @"([RSU][2345] [A-Z]+; .:\\ProgramData\\[a-z]+\\[a-z]+\.exe \[.+\[\.+])\x0B+", "$1 <==== " + UPD1 + Environment.NewLine);


        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (.+); [c-z]:\\\\Windows\\\\svchost\\.exe.*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (.+); [c-z]:\\\\.+\\" + "[\\" + NOACC + "\\])\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (U_.+); [c-z]:\\\\Program Files\\\\\\2\\\\\\2\\.exe.*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345].+; [c-z]:\\\\Program Files\\\\\\w{8}-\\w{10}-\\w{4}-\\w{4}-\\w{12}\\\\\\w+\\.tmp .*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345].+\\((technologie\\w+\\.com|PC DRIVERS HEADQUARTERS).*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] (A\\w+Service); [c-z]:\\\\Program Files\\\\\\w+soft\\\\\\2\\.exe \\[[^]]+\\].+\\[[^]]+\\].*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        // Additional conditions for checking TermService and svchost.exe
        if (Regex.IsMatch(REGEXPR, "(?i)[RSU][2345] TermService;") && !Regex.IsMatch(REGEXPR, "(?i)\\\\System32\\\\termsrv\\.dll"))
        {
            REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][2345] TermService; .+)\\v+", "$1 <==== " + UPD1 + " (" + NO + " ServiceDLL)" + Environment.NewLine);
        }

        if (BOOTM != "recovery" && UserUtils.IsUserAdministrator())
        {
            REGEXPR = Regex.Replace(REGEXPR, "(?i)([SU][2345] (?!.+(Service|UserSVC)_).+?; [c-z]:\\\\Windows\\\\System32\\\\svchost\\.exe.*)\\v+", "$1 <==== " + UPD1 + " (" + NO + " ServiceDLL)" + Environment.NewLine);
        }

        // Output or log the modified REGEXPR string
        Console.WriteLine(REGEXPR); // Replace with your actual logging or file writing logic
    }

    public static readonly HashSet<string> BlacklistedServices = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "SomeMaliciousService", // Replace with actual malicious service names
        "UnwantedService"
    };
}
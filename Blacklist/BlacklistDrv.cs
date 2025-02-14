using System;
using System.Text.RegularExpressions;

public class BlacklistDrv
{
    private static string REGEXPR;
    private static string UPD1 = "Attention";

    public static void BLACKLISTDRV()
    {
        // Regular expressions and replacing with the update information
        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][012345] (netfilter|webshieldfilter|browserMon|Winmon.*|udiskMgr|mrxsmb22|cytdsk|JszipProtect|UefGdstor|Uefochubsrv|TMKernel|LanmaMaster|WiserIso|ServiceMgr|MaohaWifiNetPro|iSafe.*?|NetUtils\\d{4}|drmkpro64|mwescontroller|NetUtils\\d+|MPCKpt|MPCBase|cherimoya|bsdp32|KuaiZipDrive\\d|UCGuard|pcwatch|cmwr|cmwf|fileHiders|webTinstMKTN\\d+);.+)\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][012345] .+\\\\Program Files\\\\LuDaShi\\\\.+(\\)|\\]))\\v{2}", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "([RSU][012345] .+; .+\\\\[^[]+:([^\\\\{].+))\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][012345].+; .+\\\\(SAntivirus|Segurazo|(Local|Windows)\\\\Temp|UBar|Common Files\\\\.oobzo)\\\\.*|MoriyaStreamWatchmen.sys)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        REGEXPR = Regex.Replace(REGEXPR, "(?i)([RSU][012345].+(?:韵羽健.+Driver|\\(technologie\\w+\\.com|PC DRIVERS HEADQUARTERS).*)\\v+", "$1 <==== " + UPD1 + Environment.NewLine);

        // Output or log the modified REGEXPR string
        Console.WriteLine(REGEXPR);  // Replace with your actual logging or file writing logic
    }
}
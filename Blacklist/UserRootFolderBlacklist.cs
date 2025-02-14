using System;
using System.Collections.Generic;
using System.IO;

namespace Wildlands_System_Scanner.Blacklist
{
    public class UserRootFolderBlacklist
    {
        public static void ScanForBlacklistedFolders()
        {
            // Predefined blacklist of folder names in the user root directory (case insensitive)
            HashSet<string> blacklistedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SearchIt99",
                "HyvesToolbar",
                "Spycheck",
                "Backups\\Windows\\Windows_security_backup files",
                "Windows\\Windows",
                "u-bot",
                "KakashiHatake",
                "Tencet",
                "Windows\\Windows",
                "MzData\\Exe_P803",
                "wc3pf2vs.0ta",
                "Windupdt",
                "xoft.cn",
                "explorer",
                "Windupdt",
                "Microsoft\\msiinfo",
                "fghdfgr",
                "IExplorer",
                "ShopToWin",
                "MSDCSASFCD",
                "soporte",
                "MSDCSASFCD",
                "DCSCMIN",
                "MSDCSC\\EnyydP6uxS5o",
                "MSDCSC\\qvn21nrh4mnz",
                "MSDCSC\\EnyydP6uxS5o",
                "MyIE",
                "Windupdt4",
                "PsjsrC",
                "Csrss",
                "MSDCSC\\aqi4dpKt3RjC",
                "ddabadfb",
                "Internet Explorer\\Services",
                "MSDCSC\\vj3FW0nBkcg1",
                "Outook gprvg",
                "Morzilla Firefox",
                "win32\\puff",
                "Svchost",
                "Windows\\AppLoc",
                "win32\\explore32",
                "win32\\taseron",
                "win32\\teamci",
                "FucKYouBABY",
                "FucKYouBABY",
                "Buzz_Words",
                "MSDgfCSC",
                "KeyScr",
                "NetBuyAssistant",
                "Sys_XP_Support",
                "sustem32",
                ".COMMgr",
"7zS4F4.tmp",
"Abn.gpc, Cef.gpc, gbieh.gmd, gbiehuni.dll , GBIEHCEF.DLL , gbiehabn.dll, gbpdist.dll', PChar^('Abn.gpc, Cef.gpc, gbieh.gmd, gbiehuni.dll , GBIEHCEF.DLL , gbiehabn.dll, gbpdist.dll",
"Application Data\\Microsoft\\ren",
"Application Data\\Microsoft\\windata",
"Explorer",
"Hwnd",
"InstallShield Installation Information",
"Joker",
"kernel",
"Localdir",
"Media",
"nsvcmon",
"System",
"system64",
"turbo_db",
"UsrData~",
"svcpad",
"AppTime",
"PassTools",
".AppData",
"C-76947-8457-2745",
"wlock",
"wl",
"AppTM",
"56D616E427563755",
"UserData\\18GBTPCL",
"UserData\\3FXJ7XWW",
"UserData\\MDZOHGJY",
"UserData\\W3BJQCPD",
"words",
".jnana",
"_qbothome",
"1136826769",
"AVP 2009",
"C-76947-8457-2745",
"Explorer\\Aplication\\IOSample",
"Fhyakl",
"Microsoft AData",
"Microsoft Private Data",
"revolution.exe",
"Searched",
"turbo_db\\tmp",
"vpnmon\\incoming",
"XP Deluxe Protector",
"inf",
"inf",
".msf3",
"ppt",
"powerpoints",
"downloads\\mmb",
"wmplayers",
"t3e0ilfioi3684m2nt3ps2b6lru",
"point",
"fbm",
"powerpoint",
"InstallShield Installation Information\\{A5BA14E0-7384-5991B8648CBE70A4}",
"IE",
"winloqon",
"powerpoints",
"netbeans_db",
"Acrobat",
"WINDOWS",
"4240",
"powerpresentation",
"wmv",
"inf",
"mediaplayers",
"khmgg",
"xls",
"WindowsUpdate",
"WinRAR\\%SESSIONNAME%",
"ppsq",
"updaterd",
"javacache",
"pdf",
"AptSw",
"AppWm",
"user_db\\tmp",
"turbo_db",
"8409",
"IEMediaEX",
"spkpod",
"tarantula",
"System\\WindowsUpdate",
"natpad",
".adobe",
"AppData\\Local\\VKMus",
"portable\\Recover Keys",
"P-7-78-8964-9648-3874",
"Microsoft-Driver-1-53-2495-3625-9745",
"AppData\\LocalFiles",
"Microsoft-Driver-1-56-9689-8752-2845",
"Microsoft-Driver-1-58-2485-3625-9745",
"Bureau\\---",
"Docume",
"admiapp",
"Librarys\\wgesdwx",
"tarantula",
"SystemWindowsFirewall",
"%appda~1",
"Application DataMicrosoft",
"3hmpa190gs",
"ncftp",
"FreeWebToon",
"Joker",
"System\\WindowsUpdate\\WinSvr\\explorer",
"M-10-8754-86589h-555h5",
"APPLIC~1Microsoft",
"somedirname",
"appserve",
"boxify",
"User3000",
"msconfig",
"msdata",
"svchost",
"dll32",
"MyPersonalStuff\\feeds",
"UserPrograms\\feeds",
"MyPersonalStuff",
"Services\\killer",
".SecuritySettings",
"BINFE",
"AppData\\RoamingMicrosoft",
"skypenews",
"AppData\\OpenMin\\miner",
"yfzju",
"61g5pe5351599",
"c3nd5c71258i9bt",
"i7q5k93865j1m3",
"AppData\\wincore",
"Birdsmade",
"borin",
"c4g6n3bp2fb8",
"WindowsUpdate"

            };

            // Path to the user root directory
            string userRootPath = @"C:\Users\12565\";

            Console.WriteLine($"Scanning folders in {userRootPath}...");

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(userRootPath))
                {
                    Console.WriteLine($"Directory not found: {userRootPath}");
                    return;
                }

                // Get all directories in the user root directory and subdirectories
                string[] directories = Directory.GetDirectories(userRootPath, "*", SearchOption.AllDirectories);

                // Check each folder against the blacklist
                foreach (string directory in directories)
                {
                    string folderName = Path.GetFileName(directory);

                    if (blacklistedFolders.Contains(folderName))
                    {
                        Logger.Instance.LogPrimary($"Blacklisted folder found: {directory}");
                        // Optionally, take action (e.g., delete or quarantine the folder)
                        // Directory.Delete(directory, true); // Be cautious with this line!
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to some directories: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                Console.WriteLine($"Folder path too long: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during scanning: {ex.Message}");
            }

            Console.WriteLine("Scan complete.");
        }
    }
}

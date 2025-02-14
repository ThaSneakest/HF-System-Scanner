using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Registry;

public class WinSockHandler
{
    private static string TEMP_DIR = @"C:\Temp";  // Update as necessary

    public static void WINSOCK()
    {
        string VAR, VAL, VAL2, REGEXP, WSCHECK;
        string HWINSOCK = Path.Combine(Path.GetTempPath(), "winsock.txt");
        string KEY = @"SYSTEM\CurrentControlSet\Services\WinSock2\Parameters\";

        int I = 1;
        while (true)
        {
            WSCHECK = "";
            string COMPANY = "";
            string SIZE = "";
            string catalogEntryKeyPath = $@"{KEY}NameSpace_Catalog5\Catalog_Entries\{I}";

            using (RegistryKey catalogEntryKey = Registry.LocalMachine.OpenSubKey(catalogEntryKeyPath))
            {
                if (catalogEntryKey == null)
                    break;

                VAL = catalogEntryKey.GetValue("LibraryPath")?.ToString();
                if (VAL == null)
                    break;

                REGEXP = ProcessSystemRoot(VAL);

                VAL2 = catalogEntryKey.GetValue("ProviderId")?.ToString();

                switch (VAL2)
                {
                    case "0x409D05229E7ECF11AE5A00AA00A7112B":
                        if (REGEXP != Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"System32\mswsock.dll"))
                        {
                            WSCHECK = $" {StringConstants.UPD1}: LibraryPath {StringConstants.INTERNET5} \"%SystemRoot%\\System32\\mswsock.dll\"";
                        }
                        break;
                        // Add other cases here as needed
                }

                if (!File.Exists(REGEXP))
                {
                    SIZE = " => File Not Found";
                }
                else
                {
                    SIZE = $" [{new FileInfo(REGEXP).Length} bytes]";
                }

                File.AppendAllText(HWINSOCK, $"Winsock: Catalog5 {I} {VAL} {SIZE} {COMPANY} {WSCHECK}{Environment.NewLine}");
            }

            I++;
        }

        using (RegistryKey namespaceCatalog5Key = Registry.LocalMachine.OpenSubKey($@"{KEY}NameSpace_Catalog5"))
        {
            if (namespaceCatalog5Key != null)
            {
                string numCatalogEntries = namespaceCatalog5Key.GetValue("Num_Catalog_Entries")?.ToString();
                if (long.TryParse(numCatalogEntries, out long num) && num > I)
                {
                    File.AppendAllText(HWINSOCK, $"Winsock: -> Catalog5 - {num} entries detected. {Environment.NewLine}");
                }
            }
        }

        // Additional processing for Catalog9 entries...
        File.Delete(HWINSOCK);
    }



    private static string ProcessSystemRoot(string path)
    {
        if (Regex.IsMatch(path, "(?i)(%SystemRoot%|%windir%)"))
        {
            path = Regex.Replace(path, "(?i)%SystemRoot%|%windir%", Environment.GetEnvironmentVariable("SystemRoot"));
            path = Regex.Replace(path, "(?i)([a-z]:)", "$1\\");
        }
        if (Regex.IsMatch(path, "(?i)%Programfiles%"))
        {
            path = Regex.Replace(path, "(?i)%Programfiles%", FolderConstants.HomeDrive + "\\Program Files");
        }
        return path;
    }

    // Placeholder for the missing methods
    private static string _HEXTOSTRING(string hex)
    {
        return string.Empty;
    }

    private static void COMP(string file) { }
    private static int GUICtrlRead(object control) { return 4; }

}

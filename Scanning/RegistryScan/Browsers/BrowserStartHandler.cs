using System;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Registry;

public class BrowserStartHandler
{
    public static void BrowserStartFunc(string browser, ref string[] startMenu, string hiv)
    {
        string val5 = RegistryUtils. RegReadString(hiv + @"\SOFTWARE\Clients\StartMenuInternet\" + browser + @"\shell\open\command");
        if (!string.IsNullOrEmpty(val5))
        {
            string operaHive = string.Empty;
            if (Regex.IsMatch(browser, "(?i)Opera|Vivaldi|Yandex"))
                operaHive = "(" + hiv + ") ";

            val5 = Regex.Replace(val5, "(?i)http(s|):", "hxxp$1:");
            if (GuiCtrlRead("CHECKBOX11") == 4)
            {
                ArrayUtils.ArrayAddAlt(ref startMenu, "StartMenuInternet: " + operaHive + browser + " - " + val5, "||||");
            }
            else
            {
                if (hiv == "HKLM")
                {
                    val5 = val5.Replace("\"", string.Empty);
                    if (browser.Contains("Chrome") && val5 != Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google\\Chrome\\Application\\chrome.exe"))
                    {
                        ArrayUtils.ArrayAddAlt(ref startMenu, "StartMenuInternet: " + browser + " - " + val5, "||||");
                    }
                    else if (browser.Contains("Brave") && val5 != Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "BraveSoftware\\Brave-Browser\\Application\\brave.exe"))
                    {
                        ArrayUtils.ArrayAddAlt(ref startMenu, "StartMenuInternet: " + browser + " - " + val5, "||||");
                    }
                    // Add other cases as necessary...
                }
                else
                {
                    BrowserStartWL(hiv, browser, ref val5);
                    if (!val5.Contains(val5))
                    {
                        ArrayUtils.ArrayAddAlt(ref startMenu, "StartMenuInternet: " + operaHive + browser + " - " + val5, "||||");
                    }
                }
            }
        }
    }

   

    public static void BrowserStartOpUser(ref string ukey, ref string valData)
    {
        string name = string.Empty;
        string browser = Regex.Replace("fix", ".+?HKU.+?\\) (.+?) -.*", "$1");
        string user = Regex.Replace("fix", ".+?\\(HKU\\\\(.+?)\\).+", "$1");

        ukey = @"HKU\" + user + @"\SOFTWARE\Clients\StartMenuInternet\" + browser + @"\shell\open\command";

        // Get account name for user...
        string userLocalAppDataDir = Regex.Replace(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"(?i)([a-z]:\\.+?\\)(.+?)(\\.+)",
            "$1" + name + "$3"
        );


        if (browser.Contains("Opera"))
        {
            if (File.Exists(Path.Combine(userLocalAppDataDir, "Programs\\Opera\\Opera.exe"))) valData = "\"" + Path.Combine(userLocalAppDataDir, "Programs\\Opera\\Opera.exe") + "\"";
            if (File.Exists(Path.Combine(userLocalAppDataDir, "Programs\\Opera\\Launcher.exe"))) valData = "\"" + Path.Combine(userLocalAppDataDir, "Programs\\Opera\\Launcher.exe") + "\"";
        }
        else if (browser.Contains("Vivaldi"))
        {
            valData = "\"" + Path.Combine(userLocalAppDataDir, "Vivaldi\\Application\\vivaldi.exe") + "\"";
        }
        else if (browser.Contains("Yandex"))
        {
            valData = "\"" + Path.Combine(userLocalAppDataDir, "Yandex\\YandexBrowser\\Application\\browser.exe") + "\"";
        }
    }

    public static void BrowserStartWL(string mkey, string browser, ref string valData)
    {
        string user = Regex.Replace(mkey, "HKU\\\\(.+)", "$1");
        string userLocalAppDataDir = Regex.Replace(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"(?i)([a-z]:\\.+?\\)(.+?)(\\.+)",

            "$1" + user + "$3"
        );
        if (browser.Contains("Opera"))
        {
            valData = "\"" + Path.Combine(userLocalAppDataDir, "Programs\\Opera\\Opera.exe") + "\"" + "\"" + Path.Combine(userLocalAppDataDir, "Opera Mail\\OperaMail.exe") + "\"" + "\"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Opera\\Launcher.exe") + "\"";
        }
        else if (browser.Contains("Vivaldi"))
        {
            valData = "\"" + Path.Combine(userLocalAppDataDir, "Vivaldi\\Application\\vivaldi.exe") + "\"";
        }
        else if (browser.Contains("Yandex"))
        {
            valData = "\"" + Path.Combine(userLocalAppDataDir, "Yandex\\YandexBrowser\\Application\\browser.exe") + "\"";
        }
    }

    private static int GuiCtrlRead(string controlName)
    {
        // Example GUI control read
        return 0;
    }


}
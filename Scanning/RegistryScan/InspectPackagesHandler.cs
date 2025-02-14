using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner;

//Needs work
public class InspectPackagesHandler
{
    public static void InspectPackages()
    {
        string keyPath = @"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";
        Console.WriteLine($"Inspecting registry key: HKCR\\{keyPath}");

        using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default))
        using (var key = baseKey.OpenSubKey(keyPath))
        {
            if (key == null)
            {
                Console.WriteLine($"Registry key not found: HKCR\\{keyPath}");
                return;
            }

            var packages = new List<string>();
            foreach (var subKeyName in key.GetSubKeyNames())
            {
                var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                string packageRootFolder = (string)subKey.GetValue("PackageRootFolder");
                if (string.IsNullOrEmpty(packageRootFolder) || !Directory.Exists(packageRootFolder)) continue;

                // Read manifest content
                string manifestContent = string.Empty;
                string manifestPath = Path.Combine(packageRootFolder, "AppxManifest.xml");
                if (File.Exists(manifestPath))
                {
                    manifestContent = File.ReadAllText(manifestPath);
                }
                else
                {
                    string[] msixFiles = Directory.GetFiles(packageRootFolder, "*.msix", SearchOption.AllDirectories);
                    if (msixFiles.Length > 0)
                    {
                        string tempZip = Path.GetTempFileName();
                        File.Copy(msixFiles[0], tempZip, true);
                        manifestContent = ZipUtils.UnzipFile(tempZip, "AppxManifest.xml");
                        File.Delete(tempZip);
                    }
                }

                string dependency = string.Empty;

                // Check for Microsoft Advertising
                if (Regex.IsMatch(manifestContent, "(?i)PackageDependency Name=\"Microsoft.Advertising") ||
                    Directory.Exists(Path.Combine(packageRootFolder, "Microsoft.Advertising")) ||
                    Directory.Exists(Path.Combine(packageRootFolder, "MSAdvertisingJS")))
                {
                    dependency = " [MS Ad]";
                }

                // Check for startup tasks
                if (Regex.IsMatch(manifestContent, "(?i)(desktop|uap5):StartupTask"))
                {
                    if (Regex.IsMatch(subKeyName, @"(?i)^(MSTeams|Microsoft\.?(MicrosoftOfficeHub|PowerAutomateDesktop|StartExperiencesApp|Todos|Teams|WindowsTerminal|YourPhone|549981C3F5F10|GamingApp))_(\d+\.)+\d+(_x\d+)*(_neutral)*_+8wekyb3d8bbwe$") ||
                        Regex.IsMatch(subKeyName, @"(?i)^Microsoft.SkypeApp_(\d+\.)+\d+(_x\d+)*__kzf8qxf38zg5c$"))
                    {
                        continue;
                    }
                    dependency += " [Startup Task]";
                }

                // Apply additional regex filters
                if (Regex.IsMatch(subKeyName, @"(?i)^((WhatsNew|WebAuthBridge.*|RoomAdjustment|.*Learning|Desktop.*|Holo.*|CortanaListenUIApp|.*MobileConnect|.*VPN|.*FileManager|.*InputApp|.*Xbox.*|.*Windows.*|.*Microsoft.*|.*Clipchamp.*))_(\d+\.)+\d+(_x\d+)*(_neutral)*_+.*$"))
                {
                    continue;
                }

                // Get package creation date
                string creationDate = Directory.GetCreationTime(packageRootFolder).ToString("yyyy-MM-dd HH:mm:ss");

                // Resolve display name
                string displayName = (string)subKey.GetValue("DisplayName") ?? subKeyName;
                if (displayName.StartsWith("ms-resource:"))
                {
                    var capabilitiesKey = subKey.OpenSubKey("App\\Capabilities");
                    if (capabilitiesKey != null)
                    {
                        displayName = (string)capabilitiesKey.GetValue("ApplicationName") ?? displayName;
                    }
                }

                // Log package information
                string packageInfo = $"{displayName} -> {packageRootFolder} [{creationDate}]{dependency}";
                Logger.Instance.LogPrimary(packageInfo);
                Console.WriteLine(packageInfo);
            }

            // Save results to file
            if (packages.Count > 0)
            {
                string outputFilePath = Path.Combine(Environment.CurrentDirectory, "Packages.txt");
                File.WriteAllLines(outputFilePath, packages);
                Console.WriteLine($"Packages saved to: {outputFilePath}");
            }
        }
    }
}

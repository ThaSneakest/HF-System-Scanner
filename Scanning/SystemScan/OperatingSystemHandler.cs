using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Wildlands_System_Scanner.Registry;

public class OperatingSystemHandler
{
    public static void Part()
    {
        // GUI update (replace with the actual implementation)
        UpdateLabelText($"Scanning, ...");

        double[] memoryStats = GetMemoryStats();
        for (int i = 0; i < memoryStats.Length; i++)
        {
            memoryStats[i] = Math.Round(memoryStats[i] / 1024, 2);
        }

        string tempFileName = "Recovery".Equals(BOOTM, StringComparison.OrdinalIgnoreCase) ? "FRST.txt" : "Addition.txt";

        using (var tempFile = new StreamWriter(Path.Combine(ScriptDir, tempFileName), true))
        {
            string cpu = "", motherboard = "", bios = "";
            if (!"Recovery".Equals(BOOTM, StringComparison.OrdinalIgnoreCase))
            {
                cpu = GetProcessorInfo();
                motherboard = GetMotherboardInfo();
                bios = GetBiosInfo();
            }

            tempFile.WriteLine($"\n==================== SYSTEM INFORMATION ===========================");
            tempFile.WriteLine($"{bios}{motherboard}{cpu}");
            tempFile.WriteLine($"Memory Usage: {memoryStats[0]}%");
            tempFile.WriteLine($"Total Physical Memory: {memoryStats[1]} MB");
            tempFile.WriteLine($"Available Physical Memory: {memoryStats[2]} MB");
            tempFile.WriteLine($"Total Virtual Memory: {memoryStats[3]} MB");
            tempFile.WriteLine($"Available Virtual Memory: {memoryStats[4]} MB");
            tempFile.WriteLine("\n==================== DRIVE INFORMATION ============================");

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (!drive.IsReady) continue;

                string type = drive.DriveType.ToString();
                string label = drive.VolumeLabel;
                double totalSpace = Math.Round(drive.TotalSize / 1024.0 / 1024.0, 2);
                double freeSpace = Math.Round(drive.AvailableFreeSpace / 1024.0 / 1024.0, 2);
                string fileSystem = drive.DriveFormat;

                tempFile.WriteLine($"Drive {drive.Name} ({label}) ({type})");
                tempFile.WriteLine($"Total Space: {totalSpace} MB, Free Space: {freeSpace} MB, File System: {fileSystem}");
            }
        }

        if (OSNUM > 5.2)
        {
            // Get the output of the `bcdedit /enum {bootmgr}` command
            string bcdEditOutput = GetBcdEditOutput();

            if (string.IsNullOrEmpty(bcdEditOutput))
            {
                Console.WriteLine("BCDEdit output is empty. Unable to parse.");
                return;
            }

            // Call ParseBcdEditOutput with the output
            string systemPartition = ParseBcdEditOutput(bcdEditOutput);

            if (!string.IsNullOrEmpty(systemPartition))
            {
                Console.WriteLine($"System Partition: {systemPartition}");
            }
            else
            {
                Console.WriteLine("System partition could not be determined.");
            }
        }

        ProcessDrives();
        int driveIndex = 0; // Example: Drive index for the first physical drive

        try
        {
            string mbrInformation = OperatingSystemHandler.ProcessMbrInformation(driveIndex);

            if (!string.IsNullOrEmpty(mbrInformation))
            {
                Console.WriteLine($"MBR Information for Drive {driveIndex}:");
                Console.WriteLine(mbrInformation);
            }
            else
            {
                Console.WriteLine($"No MBR information found for Drive {driveIndex}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing the MBR: {ex.Message}");
        }

    }

    public static double[] GetMemoryStats()
    {
        // Total physical memory in MB
        var totalMemoryInKb = new PerformanceCounter("Memory", "Total Visible Memory Size").NextValue();
        var totalMemoryInMb = totalMemoryInKb / 1024;

        // Available physical memory in MB
        var availableMemoryInKb = new PerformanceCounter("Memory", "Available Bytes").NextValue() / 1024;
        var availableMemoryInMb = availableMemoryInKb / 1024;

        // Used memory in MB
        var usedMemoryInMb = totalMemoryInMb - availableMemoryInMb;

        // System cache memory (approximation in MB)
        var cacheMemoryInKb = new PerformanceCounter("Memory", "Cache Bytes").NextValue() / 1024;
        var cacheMemoryInMb = cacheMemoryInKb / 1024;

        // Committed memory (approximation in MB)
        var committedMemoryInKb = new PerformanceCounter("Memory", "Committed Bytes").NextValue() / 1024;
        var committedMemoryInMb = committedMemoryInKb / 1024;

        // Constructing the return array
        return new[]
        {
            Math.Round((usedMemoryInMb / totalMemoryInMb) * 100, 2), // Used memory percentage
            Math.Round(totalMemoryInMb, 2), // Total physical memory
            Math.Round(availableMemoryInMb, 2), // Available physical memory
            Math.Round(cacheMemoryInMb, 2), // Cache memory
            Math.Round(committedMemoryInMb, 2) // Committed memory
        };
    }

    public static string GetProcessorInfo()
    {
        try
        {
            string processorName = string.Empty;

            // Registry path for processor information
            const string registryPath = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";

            // Retrieve the processor name from the registry
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    processorName = key.GetValue("ProcessorNameString")?.ToString() ?? "Unknown Processor";
                }
            }

            return processorName;
        }
        catch (Exception ex)
        {
            return $"Error retrieving processor info: {ex.Message}";
        }
    }


    private static string GetProcessorArchitecture(ushort architecture)
    {
        switch (architecture)
        {
            case 0:
                return "x86";
            case 1:
                return "MIPS";
            case 2:
                return "Alpha";
            case 3:
                return "PowerPC";
            case 5:
                return "ARM";
            case 6:
                return "Itanium";
            case 9:
                return "x64";
            default:
                return "Unknown";
        }
    }

    public static string GetMotherboardInfo()
    {
        try
        {
            string manufacturer = "Unknown";
            string product = "Unknown";

            // Query for motherboard information
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                    product = obj["Product"]?.ToString() ?? "Unknown";
                    break; // Assume only one motherboard
                }
            }

            return $"{manufacturer} {product}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving motherboard info: {ex.Message}";
        }
    }

    public static string GetBiosInfo()
    {
        try
        {
            string vendor = "Unknown";
            string version = "Unknown";
            string releaseDate = "Unknown";

            // Query for BIOS information
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    vendor = obj["Manufacturer"]?.ToString() ?? "Unknown";
                    version = obj["SMBIOSBIOSVersion"]?.ToString() ?? "Unknown";
                    releaseDate = obj["ReleaseDate"] != null
                        ? ManagementDateTimeConverter.ToDateTime(obj["ReleaseDate"].ToString()).ToString("yyyy-MM-dd")
                        : "Unknown";
                    break; // Assume only one BIOS
                }
            }

            return $"Vendor: {vendor}, Version: {version}, Release Date: {releaseDate}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving BIOS info: {ex.Message}";
        }
    }

    public static string ParseBcdEditOutput(string bcdEditOutput)
    {
        if (string.IsNullOrWhiteSpace(bcdEditOutput))
            throw new ArgumentException("BCDEdit output cannot be null or empty.", nameof(bcdEditOutput));

        // Split the output into lines
        var lines = bcdEditOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        string systemPartition = null;

        foreach (var line in lines)
        {
            // Look for the "device" key in the output
            if (line.TrimStart().StartsWith("device", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the partition letter
                var match = System.Text.RegularExpressions.Regex.Match(line, @"Partition=([C-Z]:)");
                if (match.Success)
                {
                    systemPartition = match.Groups[1].Value;
                    break;
                }
            }
        }

        return systemPartition;
    }

    public static string GetBcdEditOutput()
    {
        // Use Process to execute the `bcdedit` command
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c bcdedit /enum {bootmgr}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    /// <summary>
    /// Processes all drives on the system and retrieves their properties.
    /// </summary>
    public static void ProcessDrives()
    {
        var drives = DriveInfo.GetDrives();

        foreach (var drive in drives)
        {
            try
            {
                // Check if the drive is ready
                if (!drive.IsReady)
                {
                    Console.WriteLine($"Drive {drive.Name} is not ready.");
                    continue;
                }

                // Drive properties
                string driveName = drive.Name;
                string driveType = drive.DriveType.ToString();
                string volumeLabel = drive.VolumeLabel;
                string fileSystem = drive.DriveFormat;
                long totalSize = drive.TotalSize / (1024 * 1024 * 1024); // Convert to GB
                long freeSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024); // Convert to GB

                // Identify system partition (example logic)
                string systemPartition = string.Empty;
                if (File.Exists(Path.Combine(driveName, "bootmgr")))
                {
                    systemPartition = "[System Partition]";
                }

                // Check for encryption or protection
                string protectionStatus = CheckDriveProtection(driveName);

                // Print drive details
                Console.WriteLine($"Drive: {driveName}");
                Console.WriteLine($"Type: {driveType}");
                Console.WriteLine($"Label: {volumeLabel}");
                Console.WriteLine($"File System: {fileSystem}");
                Console.WriteLine($"Total Size: {totalSize} GB");
                Console.WriteLine($"Free Space: {freeSpace} GB");
                Console.WriteLine($"Status: {protectionStatus} {systemPartition}");
                Console.WriteLine(new string('-', 50));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing drive {drive.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Checks if a drive is protected or encrypted.
    /// </summary>
    /// <param name="driveName">The drive name (e.g., "C:\").</param>
    /// <returns>A string indicating the protection status.</returns>
    public static string CheckDriveProtection(string driveName)
    {
        try
        {
            // Check if "System Volume Information" exists on the drive (common for protected drives)
            string systemVolumePath = Path.Combine(driveName, "System Volume Information");
            if (Directory.Exists(systemVolumePath))
            {
                if (HasRestrictedAccess(systemVolumePath))
                {
                    return "(Protected)";
                }
            }

            // Additional logic for checking encryption or protection
            // Example: Check for BitLocker status (Windows-only)
            if (IsBitLockerEnabled(driveName))
            {
                return "(Encrypted)";
            }

            return "(Unprotected)";
        }
        catch (Exception ex)
        {
            // Log the error and return a default message
            Console.WriteLine($"Error checking drive protection for {driveName}: {ex.Message}");
            return "(Unknown Status)";
        }
    }

    /// <summary>
    /// Determines if a directory has restricted access.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>True if access is restricted; otherwise, false.</returns>
    private static bool HasRestrictedAccess(string path)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(path);
            var accessControl = directoryInfo.GetAccessControl();

            // Check for access rules
            var rules = accessControl.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
            foreach (FileSystemAccessRule rule in rules)
            {
                if ((rule.FileSystemRights & FileSystemRights.FullControl) != FileSystemRights.FullControl)
                {
                    return true; // Restricted access found
                }
            }

            return false; // No restricted access
        }
        catch
        {
            // If an exception occurs, assume access is restricted
            return true;
        }
    }

    /// <summary>
    /// Checks if BitLocker encryption is enabled on the drive.
    /// </summary>
    /// <param name="driveName">The drive name (e.g., "C:\").</param>
    /// <returns>True if BitLocker is enabled; otherwise, false.</returns>
    private static bool IsBitLockerEnabled(string driveName)
    {
        // Using WMI query to check for BitLocker status
        try
        {
            var query = $"SELECT * FROM Win32_EncryptableVolume WHERE DriveLetter = '{driveName.TrimEnd('\\')}'";
            var searcher = new System.Management.ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption", query);
            foreach (var result in searcher.Get())
            {
                uint protectionStatus = (uint)result["ProtectionStatus"];
                if (protectionStatus == 1 || protectionStatus == 2)
                {
                    return true; // BitLocker is enabled
                }
            }
        }
        catch
        {
            // WMI query failed, assume BitLocker is not enabled
        }

        return false;
    }

    /// <summary>
    /// Processes Master Boot Record (MBR) information for a given drive.
    /// </summary>
    /// <param name="driveIndex">The index of the drive to process (e.g., 0 for the first drive).</param>
    public static string ProcessMbrInformation(int driveIndex)
    {
        // Path to the physical drive
        string drivePath = $"\\\\.\\PhysicalDrive{driveIndex}";

        // Logic to read the MBR or handle errors
        try
        {
            using (FileStream driveStream = new FileStream(drivePath, FileMode.Open, FileAccess.Read))
            {
                byte[] mbrData = new byte[512]; // Standard MBR size is 512 bytes
                driveStream.Read(mbrData, 0, mbrData.Length);

                // Example: Convert MBR data to a hex string for display
                return BitConverter.ToString(mbrData).Replace("-", " ");
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException("Access to the drive is denied. Run as administrator.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to process MBR for drive {driveIndex}.", ex);
        }
    }

    /// <summary>
    /// Reads the Master Boot Record (MBR) of a given drive.
    /// </summary>
    /// <param name="drivePath">The path to the drive (e.g., "\\\\.\\PhysicalDrive0").</param>
    /// <returns>A byte array containing the MBR data.</returns>
    private static byte[] ReadMbr(string drivePath)
    {
        try
        {
            using (FileStream fs = new FileStream(drivePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[512]; // Standard MBR size
                int bytesRead = fs.Read(buffer, 0, buffer.Length);
                return bytesRead == buffer.Length ? buffer : null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to read MBR: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if the MBR data indicates a potential infection.
    /// </summary>
    /// <param name="mbrHexDump">The hex dump of the MBR data.</param>
    /// <returns>True if the MBR is infected; otherwise, false.</returns>
    private static bool CheckForMbrInfection(string mbrHexDump)
    {
        // Regex patterns for common MBR infections (simplified examples)
        string[] infectionPatterns =
        {
            @"55AA", // Standard MBR signature (present in normal and infected MBRs)
            @"33C08ED0", // Example pattern of malicious code
            @"(?:E8[0-9A-F]{2}){5}" // Repeated "E8" opcodes (example)
        };

        foreach (string pattern in infectionPatterns)
        {
            if (Regex.IsMatch(mbrHexDump, pattern, RegexOptions.IgnoreCase))
            {
                return true; // Infection detected
            }
        }

        return false; // No infection detected
    }

    private static void UpdateLabelText(string text)
    {
        // Replace with the actual GUI update implementation
        Console.WriteLine(text);
    }

    // Constants used in the original script
    private static readonly string BOOTM = "Normal"; // Example value
    private static readonly string ScriptDir = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly double OSNUM = 10.0; // Example OS number
    public static string OSUPDATE()
    {
        string osUpdate = string.Empty;

        int osNum = Environment.OSVersion.Version.Major; // Assuming OSNUM is the major version number

        if (osNum == 6 && Environment.OSVersion.Version.Minor == 3) // OS version 6.3 (Windows 8.1)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                if (key != null)
                {
                    string upD = key.GetValue("BuildLabEx")?.ToString();
                    if (!string.IsNullOrEmpty(upD))
                    {
                        if (upD.Contains("140305-1710"))
                        {
                            osUpdate = "(Update) ";
                        }
                        if (System.Text.RegularExpressions.Regex.IsMatch(upD, @"9600\.\d+\."))
                        {
                            string version = System.Text.RegularExpressions.Regex.Replace(upD, @"9600\.(\d+)\..+", "$1");
                            if (int.TryParse(version, out int parsedVersion) && parsedVersion > 17031)
                            {
                                osUpdate = "(Update) ";
                            }
                        }
                    }
                }
            }
        }
        else if (osNum == 10) // OS version 10 (Windows 10)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                if (key != null)
                {
                    string upD = key.GetValue("DisplayVersion")?.ToString();
                    if (string.IsNullOrEmpty(upD))
                    {
                        upD = key.GetValue("ReleaseId")?.ToString();
                    }

                    if (!string.IsNullOrEmpty(upD))
                    {
                        osUpdate = $"({upD}) ";
                    }

                    string upDCB = key.GetValue("CurrentBuild")?.ToString();
                    string upDUBR = key.GetValue("UBR")?.ToString();

                    if (!string.IsNullOrEmpty(upDCB) && !string.IsNullOrEmpty(upDUBR))
                    {
                        osUpdate += $"{upDCB}.{upDUBR} ";
                    }
                }
            }
        }

        return osUpdate;
    }

}

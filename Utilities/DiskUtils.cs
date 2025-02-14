using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;

public static class DiskUtils
{
    
    public static long GetDiskLength(int driveNumber)
    {
        string drivePath = $@"\\.\PhysicalDrive{driveNumber}";
        IntPtr hFile = Kernel32NativeMethods.CreateFile(
            drivePath,
            0, // GENERIC_READ
            3, // FILE_SHARE_READ | FILE_SHARE_WRITE
            IntPtr.Zero,
            3, // OPEN_EXISTING
            0,
            IntPtr.Zero);

        if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
        {
            throw new IOException("Failed to open drive handle.", Marshal.GetLastWin32Error());
        }

        try
        {
            long length = 0;
            IntPtr outBuffer = Marshal.AllocHGlobal(sizeof(long));
            try
            {
                if (!Kernel32NativeMethods.DeviceIoControl(hFile, NativeMethodConstants.IOCTL_DISK_GET_LENGTH_INFO, IntPtr.Zero, 0, outBuffer, (uint)sizeof(long), out _, IntPtr.Zero))
                {
                    throw new IOException("DeviceIoControl failed.", Marshal.GetLastWin32Error());
                }

                length = Marshal.ReadInt64(outBuffer);
            }
            finally
            {
                Marshal.FreeHGlobal(outBuffer);
            }

            return length;
        }
        finally
        {
            Kernel32NativeMethods.CloseHandle(hFile);
        }
    }
    public static string GetModel(int index)
    {
        string output = string.Empty;

        // Initialize the ManagementObjectSearcher with the WMI query
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_DiskDrive WHERE Index = '" + index + "'"
        );

        // Execute the query and iterate over the results
        foreach (ManagementObject obj in searcher.Get())
        {
            // Return the Model property if found
            output = obj["Model"]?.ToString();
            break; // We expect only one result, so exit the loop
        }

        return output;
    }
    public static int GetVolumeDiskIndex(string driveLetter)
    {
        int index = -1; // Return -1 if not found

        try
        {
            // Initialize the WMI query to get all disk drives
            ManagementObjectSearcher diskDriveSearcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_DiskDrive"
            );

            // Execute the query and iterate over the results
            foreach (ManagementObject diskDrive in diskDriveSearcher.Get())
            {
                string deviceID = diskDrive["DeviceID"].ToString().Replace("\\", "\\\\");

                // Query for the disk partitions associated with the drive
                ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceID}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition"
                );

                foreach (ManagementObject partition in partitionSearcher.Get())
                {
                    // Query for the logical disks associated with the partition
                    ManagementObjectSearcher logicalDiskSearcher = new ManagementObjectSearcher(
                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_LogicalDiskToPartition"
                    );

                    foreach (ManagementObject logicalDisk in logicalDiskSearcher.Get())
                    {
                        if (logicalDisk["DeviceID"].ToString().Equals(driveLetter, StringComparison.OrdinalIgnoreCase))
                        {
                            index = Convert.ToInt32(diskDrive["Index"]);
                            return index;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetVolumeDiskIndex: {ex.Message}");
        }

        return index;
    }
    // Import Kernel32.dll for the FindFirstVolumeW, FindNextVolumeW, and FindVolumeClose functions
    

    public static void MNT(string logFilePath)
    {
        // The array to store volume names
        var volumes = new System.Collections.Generic.List<string>();

        // Initialize the FindFirstVolumeW function to start reading volumes
        StringBuilder volumeName = new StringBuilder(255);
        IntPtr handle = Kernel32NativeMethods.FindFirstVolumeW(volumeName, (uint)volumeName.Capacity);

        if (handle == IntPtr.Zero || handle == new IntPtr(-1)) return;

        volumes.Add(volumeName.ToString());

        while (true)
        {
            bool result = Kernel32NativeMethods.FindNextVolumeW(handle, volumeName, (uint)volumeName.Capacity);
            if (!result)
            {
                Kernel32NativeMethods.FindVolumeClose(handle);
                break;
            }
            volumes.Add(volumeName.ToString());
        }

        // List all drives
        var allDrives = DriveInfo.GetDrives();

        foreach (var drive in allDrives)
        {
            if (!drive.IsReady) continue;

            // Get volume name for the mount point
            StringBuilder volumeForDrive = new StringBuilder(255);
            bool isVolumeFound = Kernel32NativeMethods.GetVolumeNameForVolumeMountPointW(drive.RootDirectory.FullName, volumeForDrive, (uint)volumeForDrive.Capacity);
            if (!isVolumeFound) continue;

            // Match and clear corresponding volumes
            for (int i = 0; i < volumes.Count; i++)
            {
                if (volumes[i] == volumeForDrive.ToString())
                {
                    volumes[i] = null;
                }
            }
        }

        // Write remaining volumes to log file
        using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
        {
            foreach (var volume in volumes)
            {
                if (string.IsNullOrEmpty(volume)) continue;

                try
                {
                    DriveInfo drive = new DriveInfo(volume);
                    writer.WriteLine($"{volume} ({drive.VolumeLabel}) ({drive.DriveType}) " +
                                     $"(Total: {Math.Round(drive.TotalSize / 1024.0 / 1024 / 1024, 2)} GB) " +
                                     $"(Free: {Math.Round(drive.AvailableFreeSpace / 1024.0 / 1024 / 1024, 2)} GB) " +
                                     $"{drive.DriveFormat}");
                }
                catch (Exception ex)
                {
                    writer.WriteLine($"Error accessing drive {volume}: {ex.Message}");
                }
            }
        }
    }
}

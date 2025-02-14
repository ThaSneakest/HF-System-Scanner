using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.NativeMethods;

namespace Wildlands_System_Scanner.Backup
{
    public class VolumeMappingHandler
    {
        public static void PerformVolumeMapping(string driveLetter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(driveLetter))
                {
                    Console.WriteLine("Invalid drive letter provided.");
                    return;
                }

                string mountPoint = driveLetter.EndsWith(":") ? $"{driveLetter}\\" : $"{driveLetter}:\\";

                // Retrieve the volume name for the given mount point
                var volumeNameBuilder = new StringBuilder(260); // MAX_PATH is 260
                if (!Kernel32NativeMethods.GetVolumeNameForVolumeMountPoint(mountPoint, volumeNameBuilder, (uint)volumeNameBuilder.Capacity))
                {
                    Console.WriteLine($"Failed to get volume name for mount point '{mountPoint}'. Error: {Marshal.GetLastWin32Error()}");
                    return;
                }

                string volumeName = volumeNameBuilder.ToString();
                Console.WriteLine($"Volume Name for {mountPoint}: {volumeName}");

                // Define a new mount point
                string newMountPoint = Path.Combine("C:\\VolumeMappings", driveLetter.TrimEnd(':'));
                Directory.CreateDirectory(newMountPoint);

                // Map the volume to the new mount point
                if (!Kernel32NativeMethods.SetVolumeMountPoint(newMountPoint, volumeName))
                {
                    Console.WriteLine($"Failed to set volume mount point for '{volumeName}' to '{newMountPoint}'. Error: {Marshal.GetLastWin32Error()}");
                    return;
                }

                Console.WriteLine($"Successfully mapped volume '{volumeName}' to '{newMountPoint}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during volume mapping: {ex.Message}");
            }
        }

        public static void PerformVolumeRemapping(string driveLetter, string newMountPoint)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(driveLetter))
                {
                    Console.WriteLine("Drive letter is invalid.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(newMountPoint))
                {
                    Console.WriteLine("New mount point is invalid.");
                    return;
                }

                string mountPoint = driveLetter.EndsWith(":") ? $"{driveLetter}\\" : $"{driveLetter}:\\";
                StringBuilder volumeNameBuilder = new StringBuilder(260); // MAX_PATH is 260

                // Retrieve the volume name
                if (!Kernel32NativeMethods.GetVolumeNameForVolumeMountPoint(mountPoint, volumeNameBuilder, (uint)volumeNameBuilder.Capacity))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to get volume name for mount point '{mountPoint}'. Error code: {errorCode}");
                    return;
                }

                string volumeName = volumeNameBuilder.ToString();
                Console.WriteLine($"Volume name for '{mountPoint}' is '{volumeName}'");

                // Remove any existing mount point for the volume
                if (Directory.Exists(newMountPoint))
                {
                    if (!Kernel32NativeMethods.DeleteVolumeMountPoint(newMountPoint))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        Console.WriteLine($"Failed to delete existing mount point '{newMountPoint}'. Error code: {errorCode}");
                        return;
                    }

                    Console.WriteLine($"Removed existing mount point: {newMountPoint}");
                }

                // Ensure the new mount point directory exists
                Directory.CreateDirectory(newMountPoint);

                // Remap the volume to the new mount point
                if (!Kernel32NativeMethods.SetVolumeMountPoint(newMountPoint, volumeName))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to set volume mount point for '{volumeName}' to '{newMountPoint}'. Error code: {errorCode}");
                    return;
                }

                Console.WriteLine($"Successfully remapped volume '{volumeName}' to '{newMountPoint}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during volume remapping: {ex.Message}");
            }
        }
    }
}

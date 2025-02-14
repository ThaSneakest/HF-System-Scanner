using System;
using System.Management;
using System.Text;

namespace Wildlands_System_Scanner.Scanning
{
    public class DetailedSystemScan
    {
        private const string LogFilePath = "DetailedSystemInfoLog.txt";

        public static void LogDetailedSystemInfo()
        {
            var sb = new StringBuilder();

            try
            {
                AppendSection(sb, "OPERATING SYSTEM INFORMATION", GetOperatingSystemInfo);
                AppendSection(sb, "COMPUTER INFORMATION", GetComputerInfo);
                AppendSection(sb, "PROCESSOR INFORMATION", GetProcessorInfo);
                AppendSection(sb, "MEMORY INFORMATION", GetMemoryInfo);
                AppendSection(sb, "DISK DRIVE INFORMATION", GetDiskInfo);
                AppendSection(sb, "LOGICAL DRIVE INFORMATION", GetLogicalDrivesInfo);
                AppendSection(sb, "NETWORK ADAPTER INFORMATION", GetNetworkAdapterInfo);
                AppendSection(sb, "GRAPHICS CARD INFORMATION", GetGraphicsCardInfo);
                AppendSection(sb, "BIOS INFORMATION", GetBIOSInfo);
                AppendSection(sb, "MOTHERBOARD INFORMATION", GetMotherboardInfo);
            }
            catch (Exception ex)
            {
                sb.AppendLine("\n[ERROR]");
                sb.AppendLine($"  Error retrieving system information: {ex.Message}");
                sb.AppendLine(new string('=', 60));
            }

            Logger.Instance.LogPrimary(sb.ToString());
            Console.WriteLine($"Detailed system information logged to {LogFilePath}");
        }

        private static void AppendSection(StringBuilder sb, string sectionTitle, Func<string> infoProvider)
        {
            sb.AppendLine(new string('=', 60));
            sb.AppendLine($"[ {sectionTitle} ]");
            sb.AppendLine(new string('-', 60));
            sb.AppendLine(infoProvider());
        }

        private static string GetOperatingSystemInfo()
        {
            return QueryWMI("Win32_OperatingSystem", "Caption, Version, BuildNumber, OSArchitecture, Manufacturer, InstallDate");
        }

        private static string GetComputerInfo()
        {
            return QueryWMI("Win32_ComputerSystem", "Manufacturer, Model, SystemType, Domain, TotalPhysicalMemory");
        }

        private static string GetProcessorInfo()
        {
            return QueryWMI("Win32_Processor", "Name, Manufacturer, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors");
        }

        private static string GetMemoryInfo()
        {
            return QueryWMI("Win32_PhysicalMemory", "Capacity, Speed, Manufacturer, FormFactor");
        }

        private static string GetDiskInfo()
        {
            return QueryWMI("Win32_DiskDrive", "Model, InterfaceType, Size, Partitions, SerialNumber");
        }

        private static string GetLogicalDrivesInfo()
        {
            return QueryWMI("Win32_LogicalDisk", "DeviceID, DriveType, FileSystem, FreeSpace, Size, VolumeName");
        }

        private static string GetNetworkAdapterInfo()
        {
            return QueryWMI("Win32_NetworkAdapter", "Name, MACAddress, Speed, NetEnabled, Manufacturer");
        }

        private static string GetGraphicsCardInfo()
        {
            return QueryWMI("Win32_VideoController", "Name, AdapterRAM, DriverVersion, VideoProcessor");
        }

        private static string GetBIOSInfo()
        {
            return QueryWMI("Win32_BIOS", "Manufacturer, Name, Version, ReleaseDate");
        }

        private static string GetMotherboardInfo()
        {
            return QueryWMI("Win32_BaseBoard", "Manufacturer, Product, SerialNumber, Version");
        }

        private static string QueryWMI(string wmiClass, string properties)
        {
            var sb = new StringBuilder();

            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT {properties} FROM {wmiClass}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        // Format all properties in a single line
                        var propertyValues = new StringBuilder();
                        foreach (var prop in obj.Properties)
                        {
                            propertyValues.Append($"{prop.Name}: {prop.Value} | ");
                        }
                        sb.AppendLine(propertyValues.ToString().TrimEnd(' ', '|'));
                    }
                    sb.AppendLine(new string('-', 40));
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  [Error] Could not query {wmiClass}: {ex.Message}");
            }

            return sb.ToString();
        }
    }
}

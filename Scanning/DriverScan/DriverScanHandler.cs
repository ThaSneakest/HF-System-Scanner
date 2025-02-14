using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static Wildlands_System_Scanner.NativeMethods.Structs;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Blacklist;
using System.Linq;

public class DriverScanHandler
{
    public static void EnumerateDrivers()
    {
        IntPtr[] drivers = new IntPtr[1024];
        uint sizeNeeded;

        // Get a list of all loaded device drivers
        if (!PsapiNativeMethods.EnumDeviceDrivers(drivers, (uint)(drivers.Length * IntPtr.Size), out sizeNeeded))
        {
            Console.WriteLine("Failed to enumerate device drivers.");
            return;
        }

        uint driverCount = sizeNeeded / (uint)IntPtr.Size;
        if (driverCount == 0)
        {
            Console.WriteLine("No drivers found.");
            return;
        }

        Console.WriteLine("Drivers found:");
        for (int i = 0; i < driverCount; i++)
        {
            IntPtr driverAddress = drivers[i];
            string driverPath = GetDriverFileName(driverAddress);

            if (!string.IsNullOrEmpty(driverPath))
            {
                try
                {
                    // Resolve the driver path
                    string resolvedPath = ResolveDriverPath(driverPath);
                    string driverName = Path.GetFileName(resolvedPath);

                    // Whitelist check
                    if (DriverWhitelist.Whitelist.Any(d => string.Equals(d, driverName, StringComparison.OrdinalIgnoreCase)))

                    {
                        // Skip logging for whitelisted drivers
                        continue;
                    }

                    // Blacklist check
                    if (DriverBlacklist.Blacklist.Any(d => string.Equals(d, driverName, StringComparison.OrdinalIgnoreCase)))
                    {
                        Logger.Instance.LogPrimary($"Malicious Driver Found <---- {resolvedPath}");
                        continue;
                    }

                    // Check if the file exists
                    if (!File.Exists(resolvedPath))
                    {
                        Logger.Instance.LogPrimary($"Unknown {driverName}; {resolvedPath} [File not found] (Unknown) [File not signed]");
                        continue;
                    }

                    // Gather file information
                    long? fileSizeInBytes = FileUtils.GetFileSize(resolvedPath);
                    string fileSize = fileSizeInBytes.HasValue ? fileSizeInBytes.Value.ToString() : "Unknown";
                    string signStatus = FileUtils.IsFileSigned(resolvedPath) ? "Signed" : "File not signed";
                    string company = FileUtils.GetFileCompany(resolvedPath);

                    // Log the driver information
                    Logger.Instance.LogPrimary($"{driverName}; {resolvedPath} [{fileSize}] ({company}) [{signStatus}]");
                }
                catch (UnauthorizedAccessException)
                {
                    Logger.Instance.LogPrimary($"Unable to access driver at {driverPath}. Access denied.");
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogPrimary($"Error accessing driver at {driverPath}: {ex.Message}");
                }
            }
        }
    }

    private static string ResolveDriverPath(string driverPath)
    {
        if (driverPath.StartsWith("\\SystemRoot", StringComparison.OrdinalIgnoreCase))
        {
            string systemRoot = Environment.GetEnvironmentVariable("SystemRoot") ?? "C:\\Windows";
            return Path.Combine(systemRoot, driverPath.Substring("\\SystemRoot\\".Length));
        }
        else if (driverPath.StartsWith("\\??\\", StringComparison.OrdinalIgnoreCase))
        {
            return driverPath.Substring(4); // Remove the "\\??\\" prefix
        }
        return driverPath;
    }

    public static string GetDriverStartType(string driverName)
    {
        IntPtr scmHandle = IntPtr.Zero;
        IntPtr serviceHandle = IntPtr.Zero;

        try
        {
            scmHandle = Advapi32NativeMethods.OpenSCManager(null, null, NativeMethodConstants.SC_MANAGER_CONNECT);
            if (scmHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to open Service Control Manager.");
            }

            serviceHandle = Advapi32NativeMethods.OpenService(scmHandle, driverName, NativeMethodConstants.SERVICE_QUERY_CONFIG);
            if (serviceHandle == IntPtr.Zero)
            {
                return "Unknown"; // If service doesn't exist, return Unknown
            }

            uint bytesNeeded;
            Advapi32NativeMethods.QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out bytesNeeded);
            IntPtr configBuffer = Marshal.AllocHGlobal((int)bytesNeeded);

            try
            {
                if (!Advapi32NativeMethods.QueryServiceConfig(serviceHandle, configBuffer, bytesNeeded, out bytesNeeded))
                {
                    throw new InvalidOperationException("Failed to query service configuration.");
                }

                var serviceConfig = Marshal.PtrToStructure<QUERY_SERVICE_CONFIG>(configBuffer);
                switch (serviceConfig.dwStartType)
                {
                    case 2:
                        return "R2"; // Auto
                    case 3:
                        return "S2"; // Manual
                    case 4:
                        return "S4"; // Disabled
                    default:
                        return "Unknown";
                }
            }
            finally
            {
                Marshal.FreeHGlobal(configBuffer);
            }
        }
        catch
        {
            return "Unknown"; // Return Unknown on any failure
        }
        finally
        {
            if (serviceHandle != IntPtr.Zero) Advapi32NativeMethods.CloseServiceHandle(serviceHandle);
            if (scmHandle != IntPtr.Zero) Advapi32NativeMethods.CloseServiceHandle(scmHandle);
        }
    }

    private static string GetDriverFileName(IntPtr driverAddress)
    {
        StringBuilder fileName = new StringBuilder(260);
        if (PsapiNativeMethods.GetDeviceDriverFileName(driverAddress, fileName, fileName.Capacity) > 0)
        {
            return fileName.ToString();
        }
        return null;
    }
}
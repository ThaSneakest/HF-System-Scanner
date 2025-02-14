using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public static class CryptUtils
{
    // Cryptographic startup
    public static bool CryptStartup()
    {
        if (NativeMethodConstants._refCount == 0)
        {
            if (!Advapi32NativeMethods.CryptAcquireContext(out NativeMethodConstants._cryptContext, IntPtr.Zero, IntPtr.Zero, 24, NativeMethodConstants.CRYPT_VERIFYCONTEXT))
            {
                Console.WriteLine($"CryptAcquireContext failed. Error: {Marshal.GetLastWin32Error()}");
                return false;
            }
        }

        NativeMethodConstants._refCount++;
        return true;
    }

    // Cryptographic shutdown
    public static void CryptShutdown()
    {
        if (NativeMethodConstants._refCount > 0)
        {
            NativeMethodConstants._refCount--;
            if (NativeMethodConstants._refCount == 0 && NativeMethodConstants._cryptContext != IntPtr.Zero)
            {
                Advapi32NativeMethods.CryptReleaseContext(NativeMethodConstants._cryptContext, 0);
                NativeMethodConstants._cryptContext = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// Checks if cryptographic services are enabled on the system.
    /// </summary>
    /// <returns>True if cryptographic services are enabled, otherwise false.</returns>
    public static bool IsCryptEnabled()
    {
        try
        {
            const string cryptographicKeyPath = @"SYSTEM\CurrentControlSet\Services\CryptSvc";
            const string startValueName = "Start";

            // Open the registry key
            using (var key = Registry.LocalMachine.OpenSubKey(cryptographicKeyPath))
            {
                if (key == null)
                {
                    Console.WriteLine("Cryptographic services registry key not found.");
                    return false;
                }

                // Read the Start value
                var startValue = key.GetValue(startValueName);
                if (startValue is int startInt)
                {
                    // Check if the service is enabled
                    // 2: Automatic, 3: Manual, 4: Disabled
                    return startInt == 2 || startInt == 3;
                }

                Console.WriteLine("Start value is not a valid integer.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking cryptographic services: {ex.Message}");
            return false;
        }
    }

    // Hash data
    public static byte[] CryptHashData(byte[] data, uint algId, bool finalize = true, IntPtr existingHash = default)
    {
        CryptStartup();

        IntPtr hashHandle = existingHash;

        try
        {
            if (hashHandle == IntPtr.Zero)
            {
                if (!Advapi32NativeMethods.CryptCreateHash(NativeMethodConstants._cryptContext, algId, IntPtr.Zero, 0, out hashHandle))
                {
                    Console.WriteLine($"CryptCreateHash failed. Error: {Marshal.GetLastWin32Error()}");
                    return null;
                }
            }

            if (!Advapi32NativeMethods.CryptHashData(hashHandle, data, (uint)data.Length, 0))
            {
                Console.WriteLine($"CryptHashData failed. Error: {Marshal.GetLastWin32Error()}");
                return null;
            }

            if (finalize)
            {
                uint hashSize = 0;
                if (!Advapi32NativeMethods.CryptGetHashParam(hashHandle, 4, null, ref hashSize, 0))
                {
                    Console.WriteLine($"CryptGetHashParam (size) failed. Error: {Marshal.GetLastWin32Error()}");
                    return null;
                }

                byte[] hashValue = new byte[hashSize];
                if (!Advapi32NativeMethods.CryptGetHashParam(hashHandle, 2, hashValue, ref hashSize, 0))
                {
                    Console.WriteLine($"CryptGetHashParam (value) failed. Error: {Marshal.GetLastWin32Error()}");
                    return null;
                }

                return hashValue;
            }
        }
        finally
        {
            if (hashHandle != IntPtr.Zero && finalize)
            {
                Advapi32NativeMethods.CryptDestroyHash(hashHandle);
            }

            CryptShutdown();
        }

        return null;
    }

    // Hash file
    public static byte[] CryptHashFile(string filePath, uint algId)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found.");
            return null;
        }

        CryptStartup();

        IntPtr hashHandle = IntPtr.Zero;

        try
        {
            // Initialize hash object
            if (!Advapi32NativeMethods.CryptCreateHash(NativeMethodConstants.CryptContext, algId, IntPtr.Zero, 0, out hashHandle))
            {
                Console.WriteLine($"CryptCreateHash failed. Error: {Marshal.GetLastWin32Error()}");
                return null;
            }

            // Read the file and hash its content
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[512 * 1024]; // 512 KB buffer
                int bytesRead;

                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] chunk = new byte[bytesRead];
                    Array.Copy(buffer, chunk, bytesRead);

                    if (!Advapi32NativeMethods.CryptHashData(hashHandle, chunk, (uint)chunk.Length, 0))
                    {
                        Console.WriteLine($"CryptHashData failed. Error: {Marshal.GetLastWin32Error()}");
                        return null;
                    }
                }
            }

            // Get the hash size
            uint hashSize = 0;
            uint hashSizeLen = sizeof(uint);

            if (!Advapi32NativeMethods.CryptGetHashParam(hashHandle, 4, null, ref hashSize, 0))
            {
                Console.WriteLine($"CryptGetHashParam (size) failed. Error: {Marshal.GetLastWin32Error()}");
                return null;
            }

            // Retrieve the hash value
            byte[] hashValue = new byte[hashSize];
            if (!Advapi32NativeMethods.CryptGetHashParam(hashHandle, 2, hashValue, ref hashSize, 0))
            {
                Console.WriteLine($"CryptGetHashParam (value) failed. Error: {Marshal.GetLastWin32Error()}");
                return null;
            }

            return hashValue;
        }
        finally
        {
            if (hashHandle != IntPtr.Zero)
            {
                Advapi32NativeMethods.CryptDestroyHash(hashHandle);
            }

            CryptShutdown();
        }
    }

    public static string MD5(string filePath)
    {
        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(fileStream);
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));
                }
                return hashStringBuilder.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating MD5: {ex.Message}");
            return null;
        }
    }

}
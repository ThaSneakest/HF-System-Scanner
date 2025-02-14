using DevExpress.Utils.Drawing.Helpers;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.Constants;
using Wildlands_System_Scanner.Utilities;
using Wildlands_System_Scanner.NativeMethods;

public static class Utility
{

    public static int GetRandomNumber(int minValue, int maxValue)
    {
        Random rand = new Random();
        return rand.Next(minValue, maxValue + 1);
    }

    public static string ExpandEnvironmentVariables(string input)
    {
        // Expand environment variables like %TEMP% here
        return Environment.ExpandEnvironmentVariables(input);
    }

    public static string GetScriptDir()
    {
        // This function should return the script directory
        return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }

    public static string ConvertName(string name)
    {
        // Replace HTML entities with their actual characters
        name = Regex.Replace(name, "&amp;", "&");
        name = Regex.Replace(name, "&quot;", "\"");
        name = Regex.Replace(name, "&lt;", "<");
        name = Regex.Replace(name, "&gt;", ">");
        name = Regex.Replace(name, "&#178;", "²");
        name = Regex.Replace(name, "&apos;", "'");

        // Handle Unicode sequences in the format \uXXXX
        if (Regex.IsMatch(name, "\\\\u\\w{4}"))
        {
            name = Regex.Replace(name, @"\\u([0-9A-Fa-f]{4})", match =>
            {
                int unicode = Convert.ToInt32(match.Groups[1].Value, 16);
                return char.ConvertFromUtf32(unicode);
            });
        }

        // Return the long file name (equivalent of AutoIt's FileGetLongName)
        try
        {
            name = Path.GetFullPath(name);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resolving long file name: {ex.Message}");
        }

        return name;
    }

    public static string GetLocaleInfo(uint locale, uint type)
    {
        char[] buffer = new char[2048];
        int result = Kernel32NativeMethods.GetLocaleInfoW(locale, type, buffer, buffer.Length);

        if (result == 0)
            throw new InvalidOperationException("Failed to retrieve locale info.");

        return new string(buffer, 0, result - 1);
    }

    
    public static string ReplaceCaseInsensitive(string source, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue))
        {
            return source;
        }

        int index = source.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return source; // No match found
        }

        var result = new StringBuilder();
        int oldValueLength = oldValue.Length;
        int startIndex = 0;

        while (index >= 0)
        {
            // Append the part before the match
            result.Append(source, startIndex, index - startIndex);

            // Append the replacement
            result.Append(newValue);

            // Move past the replaced part
            startIndex = index + oldValueLength;

            // Find the next match
            index = source.IndexOf(oldValue, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        // Append the remaining part of the string
        result.Append(source, startIndex, source.Length - startIndex);

        return result.ToString();
    }
   

    public static string GetMD5Hash(string filePath)
    {
        using (MD5 md5 = MD5.Create())
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }

    public static string CalculateMD5(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static void CheckMultipleOperatingSystems()
    {
        int count = 0;
        var drives = DriveInfo.GetDrives();

        foreach (var drive in drives)
        {
            if (drive.DriveType == DriveType.Fixed &&
                !drive.Name.Contains("X") &&
                File.Exists(Path.Combine(drive.Name, "Windows", "System32", "config", "software")))
            {
                count++;
            }
        }

        if (count > 1)
        {
            Console.WriteLine("More than one Windows operating system detected.");
            PerformAdditionalOperation();
        }
    }

    public static string GetUserProfileName()
    {
        int index = 0;

        while (true)
        {
            try
            {
                string userKeyName = Registry.Users.GetSubKeyNames()[index];
                if (userKeyName.EndsWith("_Classes", StringComparison.OrdinalIgnoreCase))
                {
                    using (var subKey = Registry.Users.OpenSubKey($"{userKeyName}\\CLSID\\{{56FDF344-FD6D-11d0-958A-006097C9A090}}\\InprocServer32"))
                    {
                        if (subKey != null)
                        {
                            string value = subKey.GetValue(null)?.ToString();
                            if (!string.IsNullOrEmpty(value) && value.IndexOf("AppData", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                return Path.GetFileNameWithoutExtension(value);
                            }
                        }
                    }
                }

                index++;
            }
            catch
            {
                break;
            }
        }

        return string.Empty;
    }


    public static string GetOperatingSystemVersion()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject os in searcher.Get())
                {
                    if (os["Caption"] != null)
                    {
                        return os["Caption"].ToString();
                    }
                }
            }
        }
        catch
        {
            // Log or handle exceptions if needed
        }

        return string.Empty;
    }

    public static string GetOperatingSystemVersionByOSVersion()
    {
        switch (Environment.OSVersion.Version.Major)
        {
            case 5:
                return "Windows XP";
            case 6:
                return Environment.OSVersion.Version.Minor == 0 ? "Windows Vista" :
                       Environment.OSVersion.Version.Minor == 1 ? "Windows 7" :
                       Environment.OSVersion.Version.Minor == 2 ? "Windows 8" :
                       "Windows 8.1";
            case 10:
                return "Windows 10";
            case 11:
                return "Windows 11";
            default:
                return "Unknown Windows Version";
        }
    }

    private static void PerformAdditionalOperation()
    {
        // Add logic for additional operations here
        Console.WriteLine("Performing additional operations...");
    }

    public static double GetOSVersion()
    {
        Version version = Environment.OSVersion.Version;

        switch (version.Major)
        {
            case 5:
                return version.Minor == 1 ? 5.1 : 0; // WIN XP
            case 6:
                switch (version.Minor)
                {
                    case 0:
                        return 6;   // WIN Vista
                    case 1:
                        return 6.1; // WIN 7
                    case 2:
                        return 6.2; // WIN 8
                    case 3:
                        return 6.3; // WIN 8.1
                    default:
                        return 0;
                }
            case 10:
                return 10; // WIN 10 and 11
            default:
                return 0;
        }
    }
    public static string GenerateRandomString(int length = 4)
    {
        var random = new Random();
        var chars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }
    public static string PtrToStringUTF8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return null;

        // Find the length of the string (null-terminated)
        int length = 0;
        while (Marshal.ReadByte(ptr, length) != 0)
        {
            length++;
        }

        // Allocate a buffer and copy the data
        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);

        // Convert to a managed string
        return Encoding.UTF8.GetString(buffer);
    }
    public static string GetUserGroup(int num)
    {
        switch (num)
        {
            case 2:
                return "Administrator";
            default:
                return "Limited";
        }
    }

    public static string ConvertSize(long sizeInBytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = sizeInBytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public static string ConvertPath(string path)
    {
        return path.Replace("http:", "hxxp:").Replace("https:", "hxxps:");
    }

    public static bool StatusSuccess(int ret)
    {
        if (ret >= 0 && ret <= 2147483647)
        {
            return true;
        }
        return false;
    }

    public static string StringToBinary(string input)
    {
        StringBuilder binaryBuilder = new StringBuilder();
        foreach (char c in input)
        {
            binaryBuilder.Append(Convert.ToString(c, 16).PadLeft(2, '0'));
        }
        return "0x" + binaryBuilder.ToString();
    }

    public static string UnicodeToString(string input)
    {
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < input.Length; i += 4)
        {
            string unicodeHex = input.Substring(i, 4);
            int charCode = Convert.ToInt32(unicodeHex, 16);
            result.Append((char)charCode);
        }
        return result.ToString();
    }

    public static string StringToUnicode(string input)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(input);
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    public static string ExpandPath(string path)
    {
        return Environment.ExpandEnvironmentVariables(path);
    }

    public static double GetOsVersion()
    {
        // Replace with logic to get the OS version (e.g., from Environment.OSVersion.Version)
        Version version = Environment.OSVersion.Version;
        return version.Major + version.Minor / 10.0;
    }

    public static string CheckForIssues(string valueName, string valueData)
    {
        string attention = string.Empty;

        // Add conditions to identify potential issues
        if (Regex.IsMatch(valueName, @"(?i)^(audiodg|explorer|lsass|services)$"))
        {
            attention = $" <==== {StringConstants.UPD1}";
        }
        if (Regex.IsMatch(valueData, @"(?i)\\Temp\\") && !valueData.Contains(@"\spool\"))
        {
            attention = $" <==== {StringConstants.UPD1}";
        }

        return attention;
    }
    public static string GetLanguageCode(string cultureInfoName)
    {
        // Extract the language code from the culture info
        if (string.IsNullOrEmpty(cultureInfoName)) return "en"; // Default to English
        return cultureInfoName.StartsWith("0x") ? cultureInfoName.Substring(2) : cultureInfoName;
    }
    public static string ConvertToUnicode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Encode the input string to Unicode
        return string.Join("", input.Select(c => $"\\u{(int)c:X4}"));
    }
    // Helper method to generate a random string
    public static string GenerateRandomFileName()
    {
        Random random = new Random();
        return random.Next(100, 999).ToString() + random.Next(100, 999).ToString();
    }

    public static string GetLocale(string locale)
    {
        if (string.IsNullOrEmpty(locale))
            return string.Empty;

        switch (locale)
        {
            case "en":
                return "English";
            case "fr":
                return "French";
            case "es":
                return "Spanish";
            default:
                return locale; // Return the original locale if no mapping exists
        }
    }

    // Example method to determine if the system is in "recovery mode"
    public static bool IsRecoveryMode()
    {
        // You could check specific conditions, such as a registry key, environment variable, etc.

        // Example: Check for a registry key or value that might indicate recovery mode
        string registryPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SystemRecovery";
        string registryValue = "RecoveryMode"; // Hypothetical value that indicates recovery mode

        var regKey = Registry.GetValue(registryPath, registryValue, null);

        if (regKey != null && regKey.ToString() == "1")
        {
            return true; // Recovery mode is enabled
        }

        return false; // Default to false if not in recovery mode
    }
    public static Structs.UNICODE_STRING StringToUnicodeString(string str)
    {
        Structs.UNICODE_STRING unicodeString = new Structs.UNICODE_STRING();
        unicodeString.Length = (ushort)(str.Length * 2);
        unicodeString.MaximumLength = (ushort)((str.Length + 1) * 2);
        unicodeString.Buffer = Marshal.StringToHGlobalUni(str);
        return unicodeString;
    }

    // Method to check for invalid characters
    public static bool CHKINVAL(string input, out string invalidChars)
    {
        // Define a list of invalid characters (you can customize this as needed)
        char[] invalidCharacterArray = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        // Initialize a variable to hold the invalid characters found
        invalidChars = string.Empty;

        // Iterate over each character in the input string
        foreach (char c in input)
        {
            if (invalidCharacterArray.Contains(c))
            {
                invalidChars += c;  // Append the invalid character to the string
            }
        }

        // Return true if invalid characters are found, otherwise false
        return !string.IsNullOrEmpty(invalidChars);
    }

    /// <summary>
    /// Converts a multibyte string (e.g., UTF-8) to a wide-character string (UTF-16).
    /// </summary>
    /// <param name="multiByteText">The multibyte text as a byte array.</param>
    /// <returns>The converted wide-character string.</returns>
    public static string MultiByteToWideChar(byte[] multiByteText)
    {
        if (multiByteText == null || multiByteText.Length == 0)
        {
            throw new ArgumentNullException(nameof(multiByteText), "Input byte array cannot be null or empty.");
        }

        // Allocate space for the wide-character string
        int wideCharCount = Kernel32NativeMethods.MultiByteToWideChar(
            65001, // UTF-8 code page
            0,     // No flags
            multiByteText,
            multiByteText.Length,
            null,
            0
        );

        if (wideCharCount == 0)
        {
            throw new InvalidOperationException("Failed to calculate the size of the wide-character string.");
        }

        // Allocate buffer for the wide-character string
        char[] wideChars = new char[wideCharCount];
        int result = Kernel32NativeMethods.MultiByteToWideChar(
            65001, // UTF-8 code page
            0,     // No flags
            multiByteText,
            multiByteText.Length,
            wideChars,
            wideChars.Length
        );

        if (result == 0)
        {
            throw new InvalidOperationException($"Failed to convert to wide-character string. Error code: {Marshal.GetLastWin32Error()}");
        }

        return new string(wideChars);
    }

    // Wrapper method to call lstrlenW
    public static int GetStringLengthW(IntPtr stringPointer)
    {
        if (stringPointer == IntPtr.Zero)
            throw new ArgumentNullException(nameof(stringPointer), "Pointer cannot be null.");

        return Kernel32NativeMethods.lstrlenW(stringPointer);
    }

    public static string MultiByteToWideChar(byte[] input, uint codePage = 0, uint flags = 0)
    {
        int charCount = Kernel32NativeMethods.MultiByteToWideChar(codePage, flags, input, input.Length, null, 0);
        if (charCount == 0)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        char[] wideChars = new char[charCount];
        if (Kernel32NativeMethods.MultiByteToWideChar(codePage, flags, input, input.Length, wideChars, wideChars.Length) == 0)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        return new string(wideChars);
    }


    public static IntPtr LoadLibraryEx(string filePath, uint flags = 0)
    {
        IntPtr result = Kernel32NativeMethods.LoadLibraryExW(filePath, IntPtr.Zero, flags);

        if (result == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to load library: {filePath}");

        return result;
    }


    public static string ExpandEnvironmentStrings(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        char[] buffer = new char[4096];
        uint result = Kernel32NativeMethods.ExpandEnvironmentStringsW(input, buffer, (uint)buffer.Length);

        if (result == 0 || result > buffer.Length)
        {
            throw new InvalidOperationException("Failed to expand environment strings.");
        }

        return new string(buffer, 0, (int)result - 1); // Remove the null terminator
    }

    public static string MultiByteToWideChar(string text, uint codePage = 0, uint flags = 0)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        int requiredSize = Kernel32NativeMethods.MultiByteToWideChar(codePage, flags, IntPtr.Zero, text.Length, IntPtr.Zero, 0);
        if (requiredSize == 0)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        IntPtr buffer = Marshal.AllocHGlobal(requiredSize * sizeof(char));
        try
        {
            int result = Kernel32NativeMethods.MultiByteToWideChar(codePage, flags, Marshal.StringToHGlobalAnsi(text), text.Length, buffer, requiredSize);
            if (result == 0)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            return Marshal.PtrToStringUni(buffer, result);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
    public static string ToHexString(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder hex = new StringBuilder(data.Length * 2);
        foreach (byte b in data)
        {
            hex.AppendFormat("{0:X2}", b);
        }
        return hex.ToString();
    }

}
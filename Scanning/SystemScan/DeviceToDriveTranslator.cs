using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public static class DeviceToDriveTranslator
{
    private static List<Tuple<string, string>> _deviceToDriveMap = new List<Tuple<string, string>>();
    private static bool _isInitialized = false;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

    public static bool BuildDeviceToDriveTranslationArray()
    {
        if (_isInitialized) return true;

        _deviceToDriveMap.Clear();
        var drives = DriveInfo.GetDrives();
        foreach (var drive in drives)
        {
            string driveLetter = drive.Name.ToUpper();
            var deviceName = new StringBuilder(100);
            if (QueryDosDevice(driveLetter.Substring(0, 2), deviceName, 100) == 0)
            {
                _deviceToDriveMap.Clear();
                return false;
            }
            _deviceToDriveMap.Add(new Tuple<string, string>(driveLetter, deviceName.ToString()));
        }

        _isInitialized = true;
        return true;
    }

    public static string TranslateDeviceFilename(string imageFilename, bool resetDriveMap)
    {
        if (string.IsNullOrEmpty(imageFilename)) throw new ArgumentException("Invalid image filename.");

        if (resetDriveMap)
        {
            _isInitialized = false;
        }

        if (!BuildDeviceToDriveTranslationArray())
        {
            throw new InvalidOperationException("Failed to build device-to-drive translation array.");
        }

        foreach (var mapping in _deviceToDriveMap)
        {
            if (imageFilename.StartsWith(mapping.Item2, StringComparison.OrdinalIgnoreCase))
            {
                return Utility.ReplaceCaseInsensitive(imageFilename, mapping.Item2, mapping.Item1);
            }
        }

        if (resetDriveMap)
        {
            return string.Empty;
        }

        _isInitialized = false;

        if (!BuildDeviceToDriveTranslationArray())
        {
            throw new InvalidOperationException("Failed to rebuild device-to-drive translation array.");
        }

        foreach (var mapping in _deviceToDriveMap)
        {
            if (imageFilename.StartsWith(mapping.Item2, StringComparison.OrdinalIgnoreCase))
            {
                return Utility.ReplaceCaseInsensitive(imageFilename, mapping.Item2, mapping.Item1);
            }
        }

        return string.Empty;
    }
}

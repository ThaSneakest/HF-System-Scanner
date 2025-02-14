using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

public class ADSHandler
{

 
    public static string[] ADSListNTQuery(string filePath)
    {
        if (!File.Exists(filePath))
            return Array.Empty<string>();

        var adsList = new List<string>();
        var streamData = new Structs.WIN32_FIND_STREAM_DATA();
        IntPtr findHandle = Kernel32NativeMethods.FindFirstStreamW(filePath, 0, ref streamData, 0);

        if (findHandle != IntPtr.Zero)
        {
            do
            {
                adsList.Add(streamData.StreamName);
            }
            while (Kernel32NativeMethods.FindNextStreamW(findHandle, ref streamData));

            Kernel32NativeMethods.FindClose(findHandle);
        }

        return adsList.ToArray();
    }
}

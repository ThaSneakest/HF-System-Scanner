using DevExpress.Utils.Drawing.Helpers;
using System;
using System.Runtime.InteropServices;
using Wildlands_System_Scanner.NativeMethods;


public static class ShellRecycleBinHandler
{


    public static (long size, long numItems) QueryRecycleBin(string rootPath = "")
    {
        Structs.SHQUERYRBINFO rbInfo = new Structs.SHQUERYRBINFO
        {
            cbSize = (uint)Marshal.SizeOf(typeof(Structs.SHQUERYRBINFO))
        };

        int result = Shell32NativeMethods.SHQueryRecycleBinW(rootPath, ref rbInfo);
        if (result != 0)
        {
            throw new InvalidOperationException($"SHQueryRecycleBinW failed with error code {result}");
        }

        return (rbInfo.i64Size, rbInfo.i64NumItems);
    }

    public const uint SHERB_NO_UI = 1 | 2 | 4;

    public static void EmptyRecycleBin(string rootPath = "", uint flags = SHERB_NO_UI, IntPtr hwnd = default)
    {
        int result = Shell32NativeMethods.SHEmptyRecycleBinW(hwnd, rootPath, flags);
        if (result != 0)
        {
            throw new InvalidOperationException($"SHEmptyRecycleBinW failed with error code {result}");
        }
    }
}

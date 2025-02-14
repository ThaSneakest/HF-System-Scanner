using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class VersionNativeMethods
    {
        [DllImport("version.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetFileVersionInfoSizeExW(uint dwFlags, string lptstrFilename, out IntPtr lpdwHandle);

        [DllImport("version.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetFileVersionInfoExW(uint dwFlags, string lptstrFilename, uint dwHandle, uint dwLen, IntPtr lpData);

        [DllImport("version.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetFileVersionInfoSizeW(string lptstrFilename, out IntPtr lpdwHandle);

        [DllImport("version.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetFileVersionInfoW(string lptstrFilename, uint dwHandle, uint dwLen, IntPtr lpData);

        [DllImport("version.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool VerQueryValueW(IntPtr pBlock, string lpSubBlock, out IntPtr lplpBuffer, out uint puLen);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Wildlands_System_Scanner.NativeMethods.Structs;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class WintrustNativeMethods
    {
        [DllImport("wintrust.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int CryptCATAdminAcquireContext(ref IntPtr phProv, IntPtr pvReserved, uint dwFlags);

        [DllImport("wintrust.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int CryptCATAdminCalcHashFromFileHandle(IntPtr hFile, ref uint pcbHash, IntPtr pbHash, uint dwFlags);

        [DllImport("wintrust.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WINVERIFYTRUST(string filepath);

        [DllImport("wintrust.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WinVerifyTrust(IntPtr hwnd, ref Guid pgActionID, ref WinTrustData pWinTrustData);

        [DllImport("wintrust.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int WinVerifyTrust(IntPtr hwnd, ref Guid actionId, IntPtr data);
    }
}

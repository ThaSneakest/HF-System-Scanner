using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class ShlwapiNativeMethods
    {
        [DllImport("shlwapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint SHLoadIndirectString(string pszSource, [MarshalAs(UnmanagedType.LPWStr)] char[] pszOutBuf, uint cchOutBuf, IntPtr ppvReserved);
    }
}

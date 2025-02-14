using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class WininetNativeMethods
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern long InternetSetOption(int hwnd, long dwOption, string lpBuffer, long dwBufferLength);
    }
}

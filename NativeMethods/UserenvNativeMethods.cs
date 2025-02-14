using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.NativeMethods
{
    public class UserenvNativeMethods
    {
        [DllImport("userenv.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetProfilesDirectoryW([MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpProfileDir, ref uint lpcchSize);


    }
}

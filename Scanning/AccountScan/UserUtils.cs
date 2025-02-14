using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.NativeMethods;
using Wildlands_System_Scanner.Utilities;

namespace Wildlands_System_Scanner
{
    public class UserUtils
    {
        // Fetch user accounts via NetUserEnum
        public static List<string[]> NetUserEnum(string server = "")
        {
            IntPtr bufferPtr = IntPtr.Zero;
            uint entriesRead = 0;
            uint totalEntries = 0;

            var result = Netapi32NativeMethods.NetUserEnum(server, 1, 2, ref bufferPtr, uint.MaxValue, ref entriesRead, ref totalEntries, IntPtr.Zero);

            if (result != 0) return new List<string[]>();

            var userList = new List<string[]>();

            // Parse the data here
            // Assuming the structure of user data has been successfully parsed into userList

            Netapi32NativeMethods.NetApiBufferFree(bufferPtr);
            return userList;
        }

        // Method to check if the current user is an administrator
        public static bool IsUserAdministrator()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

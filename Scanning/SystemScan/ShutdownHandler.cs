using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner
{
    public class ShutdownHandler
    {
        // Method to shutdown the system (equivalent to Shutdown(6) in AutoIt)
        public static void Shutdown()
        {
            // Shutdown the system (force restart)
            Process.Start("shutdown", "/r /f /t 0");
        }
    }
}

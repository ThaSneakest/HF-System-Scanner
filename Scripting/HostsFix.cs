using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting
{
    public class HostsFix
    {
        void HOSTSFIX()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "Drivers", "etc", "hosts");

            if (File.Exists(path))
            {
                // Grant permissions to the file (placeholder for _GRANTE equivalent)
                FileFix.GrantAccess(path);

                // Move or modify the file (placeholder for MOVEFILER equivalent)
                FileFix.MoveFile(path);
            }

            if (!File.Exists(path))
            {
                HOSTSFIX1(path);
            }
        }



        void HOSTSFIX1(string filePath)
        {
            string system32Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "Drivers", "etc");
            string logFilePath = "HFIXLOG.log"; // Replace with the actual path to your log file
            bool result = false;

            // Ensure the "etc" directory exists
            if (!Directory.Exists(system32Path))
            {
                Directory.CreateDirectory(system32Path);
            }

            // Simulate OS check (replace with actual logic for determining $OSNUM)
            double osNum = Utility.GetOSVersion(); // Example method to get OS version

            if (osNum == 6.1 || osNum == 6.2)
            {
                string defaultHostsPath = Path.Combine(system32Path, "hosts");
                try
                {
                    File.WriteAllText(defaultHostsPath, "#       127.0.0.1       localhost" + Environment.NewLine);
                    result = true;
                }
                catch
                {
                    result = false;
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(filePath, "127.0.0.1       localhost" + Environment.NewLine);
                    result = true;
                }
                catch
                {
                    result = false;
                }
            }

            // Logging and setting file permissions
            if (result)
            {
                File.AppendAllText(logFilePath, "Hosts restored." + Environment.NewLine);
                FileFix.SetDefaultFileAccess(filePath);
            }
            else
            {
                File.AppendAllText(logFilePath, "Failed to restore Hosts." + Environment.NewLine);
            }
        }
    }
}

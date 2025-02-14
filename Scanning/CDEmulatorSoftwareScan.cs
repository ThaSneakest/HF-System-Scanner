using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wildlands_System_Scanner.Constants;

namespace Wildlands_System_Scanner.Scanning
{
    public class CDEmulatorSoftwareScan
    {
        public static string ScanForCDEmulatorSoftware()
        {
            string CDEmulatorScan = "";

            string[] cdEmulatorPaths = new string[]
            {
                            Path.Combine(FolderConstants.HomeDrive, @"Program Files\Alcohol Soft"),
            Path.Combine(FolderConstants.HomeDrive, @"Program Files\Nero"),
            Path.Combine(FolderConstants.HomeDrive, @"Program Files\MagicDisk"),
            Path.Combine(FolderConstants.HomeDrive, @"Program Files\Slysoft"),
            Path.Combine(FolderConstants.HomeDrive, @"Program Files\IsoDisk"),
            Path.Combine(FolderConstants.HomeDrive, @"Program Files\Imgburn"),
            Path.Combine(FolderConstants.HomeDrive, @"Program Files\Roxio"),
            Path.Combine(FolderConstants.HomeDrive, @"Windows\System32\Drivers\sptd.sys")
            };

            foreach (var path in cdEmulatorPaths)
            {
                if (Directory.Exists(path))
                {
                    CDEmulatorScan += $"{Path.GetFileName(path)} Installed: {path}{Environment.NewLine}";
                }
                else if (File.Exists(path))
                {
                    CDEmulatorScan += $"sptd.sys Found: {path}{Environment.NewLine}";

                }
            }
            return CDEmulatorScan;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner
{
    public class WhitelistDrivers
    {
        public static string WHITELISTDRV(string input)
        {
            string C = @"C:\";  // Adjust according to your requirement
            string NO = @"";

            input = Regex.Replace(input, @"(?i)[RSU][12345] BattC; " + C + @"\\Windows\\system32\\Drivers\\BattC.sys \[.+\] \(Microsoft.+\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SU][1234] (Abiosdsk|abp480n5|adpu160m|Aha154x|aic78u2|aic78xx|AliIde|amsint|asc|asc3350p|asc3550|Atdisk|cd20xrnt|Changer|CmdIde|Cpqarray|dac2w2k|dac960nt|dpti2o|hpn|i2omgmt|i2omp|ini910u|lbrtfdc|mraid35x|PCIDump|PCIIde|PDCOMP|PDFRAME|PDRELI|PDRFRAME|perc2|ql1080|perc2hib|Ql10wnt|ql12160|ql1240|ql1280|Simbad|Sparrow|Winsock|symc810|symc8xx|sym_hi|sym_u3|TosIde|ultra|ViaIde|WDICA); " + NO + @" ImagePath\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SU][1234] Winsock;  " + NO + @" ImagePath\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] (R5U870FLx86|R5U870FUx86|5U877); " + C + @"\\Windows\\System32\\DRIVERS\\(R5U870FLx86|R5U870FUx86|5U877).sys \[.+\] \(Ricoh.*\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] adfs; " + C + @"\\Windows\\System32\\Drivers\\adfs.sys \[.+\] \(Adobe.+\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] ADM851X; " + C + @"\\Windows\\System32\\DRIVERS\\ADM851X.SYS \[.+\] \(ADMtek.+\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] aeaudio; " + C + @"\\Windows\\System32\\drivers\\aeaudio.sys \[.+\] \(Andrea.+\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] AgereSoftModem; " + C + @"\\Windows\\System32\\DRIVERS\\AGRSM.sys \[.+\] \((Agere|LSI).*\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] (aksfridge|Hardlock); " + C + @"\\Windows\\system32\\drivers\(aksfridge|Hardlock).sys \[.+\] \(Aladdin.+\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] aliide; " + C + @"\\Windows\\System32\\drivers\\aliide.sys \[.+\] \(Microsoft Windows -> Acer.+\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] (AmdLLD|PCnet|amdsbs); " + C + @"\\Windows\\System32\\DRIVERS\\(AmdLLD|amdsbs|pcntpci5).sys \[.+\] \(Microsoft Windows -> AMD.+\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] androidusb; " + C + @"\\Windows\\System32\\Drivers\\ssadadb.sys \[.+\] \(Google.*\)\v{2}", "");
            input = Regex.Replace(input, @"(?i)[SRU][01234] anvsnddrv; " + C + @"\\Windows\\System32\\drivers\\anvsnddrv.sys \[.+\] \(AnvSoft.+\)\v{2}", "");

            // Add more regex replacements as needed

            return input;
        }
    }
}

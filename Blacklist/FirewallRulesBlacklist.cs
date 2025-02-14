using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DevExpress.XtraEditors.RoundedSkinPanel;

namespace Wildlands_System_Scanner.Blacklist
{
    public class FirewallRulesBlacklist
    {
        public static readonly List<string> BlacklistFirewallRules = new List<string>
        {
            "TCP Query User{C20B3E22-86AA-4785-9D05-1210BD5E3F52}C:\\users\\12565\\desktop\\violet-v1.6\\dumps\\violet.exe",
            "UDP Query User{2B623872-A64B-4443-A3D9-7B875E904162}C:\\users\\12565\\desktop\\violet-v1.6\\dumps\\violet.exe",
            "TCP Query User{EC87EA9F-A2B2-4DF2-BA19-62842EBD05FE}C:\\users\\12565\\desktop\\violet-v1.6\\violet.exe",
            "UDP Query User{A6500E9C-8538-4331-AB61-3F7B6B44B59F}C:\\users\\12565\\desktop\\violet-v1.6\\violet.exe",
            "Test",
        };
    }
}

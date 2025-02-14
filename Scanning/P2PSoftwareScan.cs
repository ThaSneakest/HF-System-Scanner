using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Wildlands_System_Scanner.Constants;

namespace Wildlands_System_Scanner.Scanning
{
    public class P2PSoftwareScan
    {
        public static string ScanP2PSoftware()
        {
            string P2PScan = "";

            string[] P2PPaths = new string[]
            {
                 Path.Combine(FolderConstants.UserProfile, @"AppData\Roaming\utorrent"),
            Path.Combine(FolderConstants.ProgramFiles, @"Frostwire"),
            @"C:\Program Files\Limewire",
            @"C:\Program Files\Applejuice",
            @"C:\Program Files\Bittorrent",
            @"C:\Program Files\Cabos",
            @"C:\Program Files\eDonkey",
            @"C:\Program Files\eMule",
            @"C:\Program Files\Kazaa",
            @"C:\Program Files\Grokster",
            @"C:\Program Files\iMesh",
            @"C:\Program Files\Morpheus",
            @"C:\Program Files\Napster",
            @"C:\Program Files\Kontiki",
            @"C:\Program Files\Vuze",
            @"C:\Program Files\Azureus",
            @"C:\Program Files\aMule",
            @"C:\Program Files\Halite",
            @"C:\Program Files\BearShare",
            @"C:\Program Files\Acqlite",
            @"C:\Program Files\Bitcomet",
            @"C:\Program Files\Ares",
            @"C:\Program Files\Bitlord",
            @"C:\Program Files\Luckywire",
            @"C:\Program Files\Zultrax",
            @"C:\Program Files\FireTorrent",
            @"C:\Program Files\BitSpirit",
            @"C:\Program Files\NeoNapster",
            @"C:\Program Files\Hermes",
            @"C:\Program Files\Skeezy",
            @"C:\Program Files\Blubster",
            @"C:\Program Files\DreaMule",
            @"C:\Program Files\RetroShare",
            @"C:\Program Files\LimeRunner",
            @"C:\Program Files\BitTornado",
            @"C:\Program Files\SpeedLord",
            @"C:\Program Files\ShareGhost",
            @"C:\Program Files\Sharest",
            @"C:\Program Files\SharinHood",
            @"C:\Program Files\BitHost",
            @"C:\Program Files\ShareDix",
            @"C:\Program Files\Crux",
            @"C:\Program Files\Trillix",
            @"C:\Program Files\TruxShare"
            };

            foreach (var path in P2PPaths)
            {
                if (Directory.Exists(path))
                {
                    P2PScan += $"{Path.GetFileName(path)} Installed: {path}{Environment.NewLine}";
                    Logger.Instance.LogPrimary(P2PScan);
                }
            }
            return P2PScan;
        }
    }
}

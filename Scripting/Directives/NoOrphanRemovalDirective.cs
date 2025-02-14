using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class NoOrphanRemovalDirective
    {
        private static bool skipOrphanRemoval = false;

        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to your directives text file

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found: " + filePath);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("NoOrphanRemoval::"))
                    {
                        Console.WriteLine("NoOrphanRemoval:: directive found. Skipping orphaned registry entry removal.");
                        skipOrphanRemoval = true;
                        continue;
                    }

                    // Add additional directive handling logic here.
                }

                // Call the method to handle orphaned registry entries
                HandleOrphanedRegistryEntries();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void HandleOrphanedRegistryEntries()
        {
            if (skipOrphanRemoval)
            {
                Console.WriteLine("Skipping orphaned registry entry removal as per NoOrphanRemoval directive.");
                return;
            }

            try
            {
                // Logic to find and remove orphaned registry entries
                Console.WriteLine("Scanning for orphaned registry entries...");
                // Example: Add your orphaned registry scanning and removal code here
                Console.WriteLine("Orphaned registry entries removed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while handling orphaned registry entries: {ex.Message}");
            }
        }
    }
}

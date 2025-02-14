using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Scripting.Directives
{
    public class ExtraDirective
    {
        public static List<string> ExtraScanParameters = new List<string>();

        public static void ProcessDirectives()
        {
            string filePath = "commands.txt"; // Path to the directives text file

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
                    if (line.StartsWith("Extra::"))
                    {
                        string parameters = line.Substring("Extra::".Length).Trim();
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            Console.WriteLine($"Extra:: directive found. Parameters: {parameters}");
                            ParseExtraParameters(parameters);
                        }
                        else
                        {
                            Console.WriteLine("Extra:: directive found, but no parameters were specified.");
                        }
                    }
                }

                // Display all collected parameters
                Console.WriteLine("Extra scan parameters collected:");
                foreach (var param in ExtraScanParameters)
                {
                    Console.WriteLine($" - {param}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ParseExtraParameters(string parameters)
        {
            try
            {
                // Split the parameters by commas or spaces
                string[] paramsArray = parameters.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var param in paramsArray)
                {
                    if (ValidateParameter(param))
                    {
                        ExtraScanParameters.Add(param);
                        Console.WriteLine($"Parameter added: {param}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid parameter ignored: {param}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing parameters: {ex.Message}");
            }
        }

        private static bool ValidateParameter(string param)
        {
            // Example validation: Check if it's a directory, file extension, or scan flag
            if (Directory.Exists(param) || param.StartsWith("--") || param.StartsWith("*."))
            {
                return true;
            }

            Console.WriteLine($"Parameter validation failed: {param}");
            return false;
        }

        public static void AdjustScanBasedOnParameters()
        {
            Console.WriteLine("Adjusting scan process based on extra parameters...");

            foreach (var param in ExtraScanParameters)
            {
                if (Directory.Exists(param))
                {
                    Console.WriteLine($"Including directory in scan: {param}");
                    // Logic to add the directory to the scan
                }
                else if (param.StartsWith("*."))
                {
                    Console.WriteLine($"Including file type in scan: {param}");
                    // Logic to include file types based on extension
                }
                else if (param.StartsWith("--"))
                {
                    Console.WriteLine($"Enabling advanced scan option: {param}");
                    // Logic to enable advanced scan features
                }
                else
                {
                    Console.WriteLine($"Unknown parameter ignored: {param}");
                }
            }
        }
    }
}

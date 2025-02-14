using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Backup
{
    public class RecoveryHandler
    {
        /// <summary>
        /// Handles recovery or normal mode logic for the given key.
        /// </summary>
        /// <param name="key">The registry key or identifier to handle.</param>
        /// <param name="isRecoveryMode">Indicates whether recovery mode is active.</param>
        /// <returns>The modified key based on the specified mode.</returns>
        public static string HandleRecovery(string key, bool isRecoveryMode = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("Key cannot be null or empty.", nameof(key));
                }

                if (isRecoveryMode)
                {
                    // Recovery mode handling logic
                    Console.WriteLine($"Handling recovery mode for key: {key}");
                    return $"{key}_recovery"; // Modify the key for recovery mode
                }
                else
                {
                    // Normal mode handling logic
                    Console.WriteLine($"Handling normal mode for key: {key}");
                    return $"{key}_normal"; // Modify the key for normal mode
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred while handling the key: {key}", ex);
                throw; // Re-throw exception after logging to preserve stack trace
            }
        }

    }
}

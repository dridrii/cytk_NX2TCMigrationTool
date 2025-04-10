using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace cytk_NX2TCMigrationTool.src.Core.Common.Utilities
{
    public static class FileUtils
    {
        /// <summary>
        /// Generate a unique ID based on current time and salt
        /// </summary>
        /// <param name="salt">Salt value to make IDs unique</param>
        /// <param name="length">Length of ID (between 5-9)</param>
        /// <returns>A unique ID string</returns>
        public static string GenerateUniqueId(string salt, int length = 7)
        {
            // Ensure length is between 5 and 9
            length = Math.Max(5, Math.Min(9, length));

            // Use current timestamp, a random guid, and the salt to generate a unique hash
            string timeString = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string uniqueString = timeString + Guid.NewGuid().ToString() + salt;

            // Use SHA256 to generate a consistent hash from the unique string
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(uniqueString));

                // Convert to a Base64 string and remove non-alphanumeric characters
                var base64 = Convert.ToBase64String(hashBytes)
                    .Replace("/", "")
                    .Replace("+", "")
                    .Replace("=", "");

                // Return the first 'length' characters
                return base64.Substring(0, length);
            }
        }

        /// <summary>
        /// Calculate the SHA-256 checksum of a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Hex string representation of the SHA-256 hash</returns>
        public static string CalculateChecksum(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hashBytes = sha256.ComputeHash(stream);

                    // Convert to hex string
                    var sb = new StringBuilder();
                    foreach (var b in hashBytes)
                    {
                        sb.Append(b.ToString("x2"));
                    }

                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Check if a file is an NX part file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file is an NX part file, false otherwise</returns>
        public static bool IsNXPartFile(string filePath)
        {
            // NX part files have a .prt extension
            return Path.GetExtension(filePath).Equals(".prt", StringComparison.OrdinalIgnoreCase);
        }
    }
}
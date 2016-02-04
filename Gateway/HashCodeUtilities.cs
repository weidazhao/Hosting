using System;
using System.Security.Cryptography;
using System.Text;

namespace Gateway
{
    public static class HashCodeUtilities
    {
        /// <summary>
        /// http://www.codeproject.com/Articles/34309/Convert-String-to-bit-Integer
        /// </summary>
        public static long GetInt64HashCode(string value)
        {
            long hashCode = 0;

            if (!string.IsNullOrEmpty(value))
            {
                byte[] valueBytes = Encoding.Unicode.GetBytes(value);
                byte[] hashBytes;

                using (var hashProvider = new SHA256Cng())
                {
                    hashBytes = hashProvider.ComputeHash(valueBytes);
                }

                hashCode = BitConverter.ToInt64(hashBytes, 0) ^
                           BitConverter.ToInt64(hashBytes, 8) ^
                           BitConverter.ToInt64(hashBytes, 24);
            }

            return hashCode;
        }
    }
}

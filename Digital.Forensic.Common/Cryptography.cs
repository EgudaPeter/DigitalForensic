using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Forensic.Common
{
    public static class Cryptography
    {
        public static string salt { get { return "PIESoftwares@passw0rd.eTMS.Key"; } }

        public static string EncryptString(this string value)
        {
            return Encrypt(value, salt, SHA512.Create());
        }

        private static string Encrypt(string value, string salt, HashAlgorithm hashAlgorithm)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            var toHash = new byte[valueBytes.Length + saltBytes.Length];
            Array.Copy(valueBytes, 0, toHash, 0, valueBytes.Length);
            Array.Copy(saltBytes, 0, toHash, valueBytes.Length, saltBytes.Length);

            var hash = hashAlgorithm.ComputeHash(toHash);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}

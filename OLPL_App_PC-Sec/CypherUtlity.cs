using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLPL_App_PC_Sec
{
    using System;
    using System.Text;
    using System.Security.Cryptography;
    using System.IO;

    public class CipherUtility
    {
        public static string Encrypt<T>(string value, string password, string salt)
             where T : SymmetricAlgorithm, new()
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new T();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIV);

            using (MemoryStream buffer = new MemoryStream())
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    }
                }

                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        public static string Decrypt<T>(string text, string password, string salt)
           where T : SymmetricAlgorithm, new()
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new T();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIV);

            using (MemoryStream buffer = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        public static string GenerateSimpleSalt(int maxSize = 64)
        {
            var alphaSet = new char[64]; // use 62 for strict alpha... that random generator for alphas only
            //nicer results with set length * int i = 256. But still produces excellent random results.
            //alphaset plus 2.  Reduce to 62 if alpha requried
            alphaSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890#!".ToCharArray();
            var crypto = new RNGCryptoServiceProvider();
            var bytes = new byte[maxSize];
            crypto.GetBytes(bytes); //get a bucket of very random bytes
            var tempSB = new StringBuilder(maxSize);
            foreach (var b in bytes)
            {   // use b , a random from 0-255 as the index to our source array. Just mod on length set
                tempSB.Append(alphaSet[b % (alphaSet.Length)]);
            }
            return tempSB.ToString();
        }
    }
}

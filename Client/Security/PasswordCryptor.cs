using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client.Security
{
    public class PasswordCryptor
    {
        private readonly byte[] encryptionKey;
        public PasswordCryptor(string processorId)
        {
            encryptionKey = GenerateKeyFromProcessorId(processorId);
        }

        private byte[] GenerateKeyFromProcessorId(string processorId)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(processorId));
            }
        }
        public string EncryptPassword(string password)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.GenerateIV();
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new System.IO.MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(password);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string DecryptPassword(string encryptedPassword)
        {
            byte[] fullCipher = Convert.FromBase64String(encryptedPassword);

            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;

                byte[] iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new System.IO.MemoryStream(fullCipher, 16, fullCipher.Length - 16))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new System.IO.StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}

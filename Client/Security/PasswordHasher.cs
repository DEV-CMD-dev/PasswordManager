using System.Security.Cryptography;
using System.Text;

namespace Client.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Use a secure hashing algorithm (e.g., SHA256) to hash the password
            using (var sha = SHA256.Create())
            {
                var a = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return string.Join("", a);
            }
        }
    }
}

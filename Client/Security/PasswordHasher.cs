using System.Security.Cryptography;
using System.Text;

namespace Client.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var a = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(a);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Security
{
    public static class PasswordGenerator
    {
        private static readonly Random random = new Random();

        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

        public static string Generate(int length)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters");

            string allChars = Lowercase + Uppercase + Digits + SpecialChars;
            var password = new StringBuilder();

            password.Append(Lowercase[random.Next(Lowercase.Length)]);
            password.Append(Uppercase[random.Next(Uppercase.Length)]);
            password.Append(Digits[random.Next(Digits.Length)]);
            password.Append(SpecialChars[random.Next(SpecialChars.Length)]);

            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            return ShuffleString(password.ToString());
        }

        private static string ShuffleString(string input)
        {
            var array = input.ToCharArray();
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int j = random.Next(i, n);
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
            return new string(array);
        }
    }
}

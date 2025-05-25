using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.Security
{
    public static class PasswordGenerator
    {
        private static readonly Random random = new Random();

        public static string Generate(int length, bool lowercase, bool uppercase, bool digits, bool specialChars)
        {
            if (!lowercase && !uppercase && !digits && !specialChars || length <= 0)
                return string.Empty;

            const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string Digits = "0123456789";
            const string SpecialChars = "!@#$%^&*()-_=+[]{};:,.<>?";

            var allChars = new StringBuilder();
            var password = new StringBuilder();
            var random = new Random();

            if (lowercase)
            {
                password.Append(Lowercase[random.Next(Lowercase.Length)]);
                allChars.Append(Lowercase);
            }
            if (uppercase)
            {
                password.Append(Uppercase[random.Next(Uppercase.Length)]);
                allChars.Append(Uppercase);
            }
            if (digits)
            {
                password.Append(Digits[random.Next(Digits.Length)]);
                allChars.Append(Digits);
            }
            if (specialChars)
            {
                password.Append(SpecialChars[random.Next(SpecialChars.Length)]);
                allChars.Append(SpecialChars);
            }

            while (password.Length < length)
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

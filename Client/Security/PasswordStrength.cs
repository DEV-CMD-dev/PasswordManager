﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client.Security
{
    public enum PasswordStrength
    {
        VeryWeak = 1,
        Weak,
        Medium,
        Strong,
        VeryStrong
    }

    public class PasswordStrengthEvaluator
    {
        public static PasswordStrength Evaluate(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return PasswordStrength.VeryWeak;

            int score = 0;

            if (password.Length >= 8) score += 10;
            if (password.Length >= 12) score += 10;
            if (password.Length >= 16) score += 5;

            if (Regex.IsMatch(password, @"[a-z]")) score += 5;
            if (Regex.IsMatch(password, @"[A-Z]")) score += 10;
            if (Regex.IsMatch(password, @"[0-9]")) score += 10;
            if (Regex.IsMatch(password, @"[\W_]")) score += 15;

            int uniqueChars = password.Distinct().Count();
            score += uniqueChars >= 8 ? 10 : uniqueChars;

            if (password.Length < 6) score -= 10;

            if (score < 20)
                return PasswordStrength.VeryWeak;
            if (score < 40)
                return PasswordStrength.Weak;
            if (score < 60)
                return PasswordStrength.Medium;
            if (score < 70)
                return PasswordStrength.Strong;

            return PasswordStrength.VeryStrong;
        }

        public static string GetStrengthText(PasswordStrength strength)
        {
            switch (strength)
            {
                case PasswordStrength.VeryWeak:
                    return "Very Weak";
                case PasswordStrength.Weak:
                    return "Weak";
                case PasswordStrength.Medium:
                    return "Medium";
                case PasswordStrength.Strong:
                    return "Strong";
                case PasswordStrength.VeryStrong:
                    return "Very Strong";
                default:
                    return "Unknown";
            }
        }
        
        public static string GetStrengthStars(PasswordStrength strength)
        {
            int stars = (int)strength;
            return new string('*', stars) + new string('#', 5 - stars);
        }
    }
}
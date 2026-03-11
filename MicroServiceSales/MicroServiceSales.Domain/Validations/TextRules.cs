using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MicroServiceSales.Domain.Validations
{
    public static class TextRules
    {
        private static readonly Regex LettersOnly = new("^[A-Za-z¡…Õ”⁄·ÈÌÛ˙—Ò]+$", RegexOptions.Compiled);
        private static readonly Regex LettersAndSpaces = new("^[A-Za-z¡…Õ”⁄·ÈÌÛ˙—Ò\\s]+$", RegexOptions.Compiled);
        private static readonly Regex EmailPattern = new("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SentenceCleaner = new("\\s+", RegexOptions.Compiled);

        public static string NormalizeSpaces(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return SentenceCleaner.Replace(s.Trim(), " ");
        }

        public static string CanonicalSentence(string? s)
        {
            var norm = NormalizeSpaces(s);
            if (string.IsNullOrEmpty(norm)) return string.Empty;
            return char.ToUpper(norm[0]) + norm.Substring(1);
        }

        public static bool IsValidLettersOnly(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return LettersOnly.IsMatch(s);
        }

        public static bool IsValidLettersAndSpaces(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return LettersAndSpaces.IsMatch(s);
        }

        public static bool IsValidEmail(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return EmailPattern.IsMatch(s);
        }
    }
}

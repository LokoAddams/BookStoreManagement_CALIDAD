using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MicroServiceDistributors.Domain.Validations
{
    public static class TextRules
    {
        private static readonly Regex LettersOnly = new(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ]+$", RegexOptions.Compiled);
        private static readonly Regex LettersAndSpaces = new(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", RegexOptions.Compiled);
        private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PhoneDigits = new(@"^\d{8}$", RegexOptions.Compiled);
        private static readonly Regex SentenceCleaner = new(@"\s+", RegexOptions.Compiled);

        public static string NormalizeSpaces(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return SentenceCleaner.Replace(s.Trim(), " ");
        }

        public static string CanonicalPersonName(string? s)
        {
            var norm = NormalizeSpaces(s);
            if (string.IsNullOrEmpty(norm)) return string.Empty;
            return char.ToUpper(norm[0]) + norm.Substring(1).ToLower();
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

        public static bool IsValidPhone(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return PhoneDigits.IsMatch(s);
        }

        public static bool IsValidProductDescriptionLoose(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return Regex.IsMatch(s, @"^[A-Za-zÁÉÍÓÚáéíóúÑñ0-9\s\.,\-]+$");
        }

        public static string CanonicalProductName(string? s)
        {
            var norm = NormalizeSpaces(s);
            if (string.IsNullOrEmpty(norm)) return string.Empty;
            return char.ToUpper(norm[0]) + norm.Substring(1);
        }

        public static IEnumerable<string> GetProductNameErrors(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                yield return "El nombre es obligatorio.";
                yield break;
            }

            if (name.Length > 100)
                yield return "El nombre no debe superar los 100 caracteres.";

            if (!Regex.IsMatch(name, @"^[A-Za-zÁÉÍÓÚáéíóúÑñ0-9\s]+$"))
                yield return "El nombre contiene caracteres inválidos.";
        }
    }
}

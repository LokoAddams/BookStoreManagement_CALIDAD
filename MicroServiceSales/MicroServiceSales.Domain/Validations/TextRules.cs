using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MicroServiceSales.Domain.Validations
{
    public static class TextRules
    {
        private static readonly Regex SentenceCleaner = new("\\s+", RegexOptions.Compiled);

        public static string NormalizeSpaces(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return SentenceCleaner.Replace(s.Trim(), " ");
        }

    }
}

using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceUsers.Domain.Validations
{
    public static class UserValidation
    {
        private const int UsernameMaxLength = 50;
        private const int FirstNameMaxLength = 50;
        private const int LastNameMaxLength = 100;
        private const int MiddleNameMaxLength = 50;
        private const int EmailMaxLength = 150;

        public static void Normalize(User u)
        {
            u.Username = u.Username?.Trim().ToLowerInvariant() ?? string.Empty;
            u.Email = u.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(u.FirstName))
                u.FirstName = TextRules.CanonicalPersonName(u.FirstName);
            if (!string.IsNullOrWhiteSpace(u.LastName))
                u.LastName = TextRules.CanonicalPersonName(u.LastName);
            if (!string.IsNullOrWhiteSpace(u.MiddleName))
                u.MiddleName = TextRules.CanonicalPersonName(u.MiddleName);
        }

        public static IEnumerable<ValidationError> Validate(User u)
        {
            // 1. Validar Username
            var username = TextRules.NormalizeSpaces(u.Username);
            foreach (var error in ValidateField(nameof(u.Username), username, isRequired: true, UsernameMaxLength)) yield return error;
            if (!string.IsNullOrWhiteSpace(username) && !TextRules.IsValidUsername(username))
                yield return new ValidationError(nameof(u.Username), "El nombre de usuario solo puede contener letras, números, puntos y guiones bajos.");

            // 2. Validar Email
            var email = u.Email?.Trim();
            foreach (var error in ValidateField(nameof(u.Email), email, isRequired: true, EmailMaxLength)) yield return error;
            if (!string.IsNullOrWhiteSpace(email) && !TextRules.IsValidEmail(email))
                yield return new ValidationError(nameof(u.Email), "Debe ingresar un correo electrónico válido.");

            // 3. Validar Nombres 
            foreach (var error in ValidateNameField(nameof(u.FirstName), u.FirstName, FirstNameMaxLength, "El nombre")) yield return error;
            foreach (var error in ValidateNameField(nameof(u.LastName), u.LastName, LastNameMaxLength, "El apellido", allowSpaces: true)) yield return error;
            foreach (var error in ValidateNameField(nameof(u.MiddleName), u.MiddleName, MiddleNameMaxLength, "El segundo nombre")) yield return error;
        }


        private static IEnumerable<ValidationError> ValidateField(string fieldName, string? value, bool isRequired, int maxLength)
        {
            if (isRequired && string.IsNullOrWhiteSpace(value))
                yield return new ValidationError(fieldName, $"El {fieldName} es obligatorio.");
            else if (value?.Length > maxLength)
                yield return new ValidationError(fieldName, $"No debe superar {maxLength} caracteres.");
        }

        private static IEnumerable<ValidationError> ValidateNameField(string fieldName, string? value, int maxLength, string label, bool allowSpaces = false)
        {
            if (string.IsNullOrWhiteSpace(value)) yield break;

            var normalized = TextRules.NormalizeSpaces(value);

            if (normalized.Length > maxLength)
                yield return new ValidationError(fieldName, $"{label} no debe superar {maxLength} caracteres.");

            if (!allowSpaces && normalized.Contains(' '))
                yield return new ValidationError(fieldName, $"{label} no debe contener espacios.");

            bool isValid = allowSpaces ? TextRules.IsValidLettersAndSpaces(normalized) : TextRules.IsValidLettersOnly(normalized);
            if (!isValid)
                yield return new ValidationError(fieldName, $"{label} solo puede contener letras{(allowSpaces ? " y espacios" : "")}.");
        }
    }
}

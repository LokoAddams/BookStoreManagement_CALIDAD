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
            var username = TextRules.NormalizeSpaces(u.Username);
            if (string.IsNullOrWhiteSpace(username))
                yield return new ValidationError(nameof(u.Username), "El nombre de usuario es obligatorio.");
            else if (username.Length > UsernameMaxLength)
                yield return new ValidationError(nameof(u.Username), $"El nombre de usuario no debe superar {UsernameMaxLength} caracteres.");
            else if (!TextRules.IsValidUsername(username))
                yield return new ValidationError(nameof(u.Username), "El nombre de usuario solo puede contener letras, números, puntos y guiones bajos.");

            var email = u.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                yield return new ValidationError(nameof(u.Email), "El correo electrónico es obligatorio.");
            else if (email.Length > EmailMaxLength)
                yield return new ValidationError(nameof(u.Email), $"El correo no debe superar {EmailMaxLength} caracteres.");
            else if (!TextRules.IsValidEmail(email))
                yield return new ValidationError(nameof(u.Email), "Debe ingresar un correo electrónico válido.");

            if (!string.IsNullOrWhiteSpace(u.FirstName))
            {
                var firstName = TextRules.NormalizeSpaces(u.FirstName);
                if (firstName.Contains(' '))
                    yield return new ValidationError(nameof(u.FirstName), "El nombre no debe contener espacios.");
                else if (firstName.Length > FirstNameMaxLength)
                    yield return new ValidationError(nameof(u.FirstName), $"El nombre no debe superar {FirstNameMaxLength} caracteres.");
                else if (!TextRules.IsValidLettersOnly(firstName))
                    yield return new ValidationError(nameof(u.FirstName), "El nombre solo puede contener letras.");
            }

            if (!string.IsNullOrWhiteSpace(u.LastName))
            {
                var lastName = TextRules.NormalizeSpaces(u.LastName);
                if (lastName.Length > LastNameMaxLength)
                    yield return new ValidationError(nameof(u.LastName), $"El apellido no debe superar {LastNameMaxLength} caracteres.");
                else if (!TextRules.IsValidLettersAndSpaces(lastName))
                    yield return new ValidationError(nameof(u.LastName), "El apellido solo puede contener letras y espacios.");
            }

            if (!string.IsNullOrWhiteSpace(u.MiddleName))
            {
                var middleName = TextRules.NormalizeSpaces(u.MiddleName);
                if (middleName.Length > MiddleNameMaxLength)
                    yield return new ValidationError(nameof(u.MiddleName), $"El segundo nombre no debe superar {MiddleNameMaxLength} caracteres.");
                else if (!TextRules.IsValidLettersOnly(middleName))
                    yield return new ValidationError(nameof(u.MiddleName), "El segundo nombre solo puede contener letras.");
            }

            // PasswordHash no se valida aquí ya que se genera automáticamente
        }

        public static Result ValidateAsResult(User u)
            => Result.FromValidation(Validate(u));

        public static Result<User> ValidateAndWrap(User u)
        {
            var errors = Validate(u).ToList();
            return errors.Count == 0
                ? Result<User>.Ok(u)
                : Result<User>.FromErrors(errors);
        }
    }
}

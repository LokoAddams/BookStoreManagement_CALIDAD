using MicroServiceClient.Domain.Models;
using MicroServiceClient.Domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceClient.Domain.Validations
{
    public static class ClientValidation
    {
        private const int FirstNameMaxLength = 50;
        private const int LastNameMaxLength = 100;
        private const int EmailMaxLength = 150;
        private const int AddressMaxLength = 200;
        private const int CiMaxLength = 20;

        public static void Normalize(Client c)
        {
            c.Ci = TextRules.NormalizeCi(c.Ci);
            c.FirstName = TextRules.CanonicalPersonName(c.FirstName);
            c.LastName = TextRules.CanonicalPersonName(c.LastName);
            c.Email = c.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            c.Phone = TextRules.NormalizeSpaces(c.Phone);
            c.Address = TextRules.CanonicalSentence(c.Address);
        }

        public static IEnumerable<ValidationError> Validate(Client c)
        {
            // Agregamos los resultados de cada función pequeña
            foreach (var error in ValidateCi(c.Ci)) yield return error;
            foreach (var error in ValidateFirstName(c.FirstName)) yield return error;
            foreach (var error in ValidateLastName(c.LastName)) yield return error;
            foreach (var error in ValidateEmail(c.Email)) yield return error;
            foreach (var error in ValidatePhone(c.Phone)) yield return error;
            foreach (var error in ValidateAddress(c.Address)) yield return error;
        }

        private static IEnumerable<ValidationError> ValidateCi(string ciRaw)
        {
            var ci = TextRules.NormalizeSpaces(ciRaw).ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(ci))
                yield return new ValidationError("Ci", "El CI es obligatorio.");
            else if (ci.Length > CiMaxLength)
                yield return new ValidationError("Ci", $"El CI no debe superar {CiMaxLength} caracteres.");
            else if (!TextRules.IsValidBoliviaCi(ci))
                yield return new ValidationError("Ci", "El CI debe contener solo números y una extensión válida opcional (p. ej. 1234567-CB).");
        }

        private static IEnumerable<ValidationError> ValidateFirstName(string nameRaw)
        {
            var first = TextRules.NormalizeSpaces(nameRaw);
            if (string.IsNullOrWhiteSpace(first))
                yield return new ValidationError("FirstName", "El nombre es obligatorio.");
            else if (first.Contains(' '))
                yield return new ValidationError("FirstName", "El nombre no debe contener espacios.");
            else if (first.Length > FirstNameMaxLength)
                yield return new ValidationError("FirstName", $"El nombre no debe superar {FirstNameMaxLength} caracteres.");
            else if (!TextRules.IsValidLettersOnly(first))
                yield return new ValidationError("FirstName", "El nombre solo puede contener letras.");
        }

        private static IEnumerable<ValidationError> ValidateLastName(string lastRaw)
        {
            var last = TextRules.NormalizeSpaces(lastRaw);
            if (string.IsNullOrWhiteSpace(last))
                yield return new ValidationError("LastName", "El apellido es obligatorio.");
            else if (last.Length > LastNameMaxLength)
                yield return new ValidationError("LastName", $"El apellido no debe superar {LastNameMaxLength} caracteres.");
            else if (!TextRules.IsValidLettersAndSpaces(last))
                yield return new ValidationError("LastName", "El apellido solo puede contener letras y espacios.");
        }

        private static IEnumerable<ValidationError> ValidateEmail(string emailRaw)
        {
            var email = emailRaw?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                yield return new ValidationError("Email", "El correo electrónico es obligatorio.");
            else if (email.Length > EmailMaxLength)
                yield return new ValidationError("Email", $"El correo no debe superar {EmailMaxLength} caracteres.");
            else if (!TextRules.IsValidEmail(email))
                yield return new ValidationError("Email", "Debe ingresar un correo electrónico válido.");
        }

        private static IEnumerable<ValidationError> ValidatePhone(string phoneRaw)
        {
            var phone = TextRules.NormalizeSpaces(phoneRaw);
            if (string.IsNullOrWhiteSpace(phone))
                yield return new ValidationError("Phone", "El número de teléfono es obligatorio.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{8}$"))
                yield return new ValidationError("Phone", "El número de teléfono debe tener exactamente 8 dígitos.");
        }

        private static IEnumerable<ValidationError> ValidateAddress(string addressRaw)
        {
            var address = TextRules.NormalizeSpaces(addressRaw);
            if (string.IsNullOrWhiteSpace(address))
                yield return new ValidationError("Address", "La dirección es obligatoria.");
            else if (address.Length > AddressMaxLength)
                yield return new ValidationError("Address", $"La dirección no debe superar {AddressMaxLength} caracteres.");
        }

        public static Result ValidateAsResult(Client c)
            => Result.FromValidation(Validate(c));

        public static Result<Client> ValidateAndWrap(Client c)
        {
            var errors = Validate(c).ToList();
            return errors.Count == 0
                ? Result<Client>.Ok(c)
                : Result<Client>.FromErrors(errors);
        }
    }
}

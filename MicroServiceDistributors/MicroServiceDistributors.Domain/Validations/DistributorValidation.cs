using MicroServiceDistributors.Domain.Models;
using MicroServiceDistributors.Domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceDistributors.Domain.Validations
{
    public static class DistributorValidation
    {
        private const int NameMaxLength = 100;
        private const int EmailMaxLength = 150;
        private const int AddressMaxLength = 200;

        public static void Normalize(Distributor d)
        {
            d.Name = TextRules.CanonicalSentence(d.Name);
            d.ContactEmail = d.ContactEmail?.Trim().ToLowerInvariant() ?? string.Empty;
            d.Phone = TextRules.NormalizeSpaces(d.Phone);
            d.Address = TextRules.CanonicalSentence(d.Address);
        }

        public static IEnumerable<ValidationError> Validate(Distributor d)
        {
            var name = TextRules.NormalizeSpaces(d.Name);
            if (string.IsNullOrWhiteSpace(name))
                yield return new ValidationError(nameof(d.Name), "El nombre es obligatorio.");
            else if (name.Length > NameMaxLength)
                yield return new ValidationError(nameof(d.Name), $"El nombre no debe superar {NameMaxLength} caracteres.");
            else if (!TextRules.IsValidProductDescriptionLoose(name))
                yield return new ValidationError(nameof(d.Name), "El nombre contiene caracteres inválidos.");

            var email = d.ContactEmail?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                yield return new ValidationError(nameof(d.ContactEmail), "El correo electrónico es obligatorio.");
            else if (email.Length > EmailMaxLength)
                yield return new ValidationError(nameof(d.ContactEmail), $"El correo no debe superar {EmailMaxLength} caracteres.");
            else if (!TextRules.IsValidEmail(email))
                yield return new ValidationError(nameof(d.ContactEmail), "Debe ingresar un correo electrónico válido.");

            var phone = TextRules.NormalizeSpaces(d.Phone);
            if (string.IsNullOrWhiteSpace(phone))
                yield return new ValidationError(nameof(d.Phone), "El teléfono es obligatorio.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{8}$"))
                yield return new ValidationError(nameof(d.Phone), "El teléfono debe tener exactamente 8 dígitos.");

            var address = TextRules.NormalizeSpaces(d.Address);
            if (string.IsNullOrWhiteSpace(address))
                yield return new ValidationError(nameof(d.Address), "La dirección es obligatoria.");
            else if (address.Length > AddressMaxLength)
                yield return new ValidationError(nameof(d.Address), $"La dirección no debe superar {AddressMaxLength} caracteres.");
        }

        public static Result ValidateAsResult(Distributor d)
            => Result.FromValidation(Validate(d));

        public static Result<Distributor> ValidateAndWrap(Distributor d)
        {
            var errors = Validate(d).ToList();
            return errors.Count == 0
                ? Result<Distributor>.Ok(d)
                : Result<Distributor>.FromErrors(errors);
        }
    }
}

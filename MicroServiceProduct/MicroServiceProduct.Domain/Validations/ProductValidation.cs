using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroServiceProduct.Domain.Models;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Domain.Results;

namespace MicroServiceProduct.Domain.Validations
{
    public static class ProductValidation
    {
        private const int DescriptionMaxLength = 500;
        private const int NameMaxLength = 100;
        public const decimal MaxPrice = 999999.99m;

        public static void Normalize(Product p)
        {
            p.Name = TextRules.CanonicalProductName(p.Name);
            if (!string.IsNullOrWhiteSpace(p.Description))
                p.Description = TextRules.CanonicalSentence(p.Description);
        }

        public static IEnumerable<ValidationError> Validate(Product p, ICategoryRepository categoryRepository)
        {
            var nameTrim = TextRules.NormalizeSpaces(p.Name);
            if (string.IsNullOrWhiteSpace(nameTrim))
                yield return new ValidationError(nameof(p.Name), "El nombre es obligatorio.");
            else if (nameTrim.Length > NameMaxLength)
                yield return new ValidationError(nameof(p.Name), $"El nombre no debe superar {NameMaxLength} caracteres.");
            else
            {
                foreach (var msg in TextRules.GetProductNameErrors(p.Name))
                    yield return new ValidationError(nameof(p.Name), msg);
            }

            if (p.CategoryId == Guid.Empty || categoryRepository.Read(p.CategoryId) == null)
                yield return new ValidationError(nameof(p.CategoryId), "Debe seleccionar una categoría válida.");

            var descTrim = TextRules.NormalizeSpaces(p.Description);
            if (string.IsNullOrWhiteSpace(descTrim))
                yield return new ValidationError(nameof(p.Description), "La descripción es obligatoria.");
            else if (descTrim.Length > DescriptionMaxLength)
                yield return new ValidationError(nameof(p.Description), $"La descripción no debe superar {DescriptionMaxLength} caracteres.");
            else if (!TextRules.IsValidProductDescriptionLoose(descTrim))
                yield return new ValidationError(nameof(p.Description), "La descripción contiene caracteres inválidos.");

            if (p.Price <= 0)
                yield return new ValidationError(nameof(p.Price), "El precio debe ser mayor a 0.");
            else if (p.Price > MaxPrice)
                yield return new ValidationError(nameof(p.Price), $"El precio no debe superar {MaxPrice}.");

            if (p.Stock < 0)
                yield return new ValidationError(nameof(p.Stock), "El stock no puede ser negativo.");
        }

        public static Result ValidateAsResult(Product p, ICategoryRepository categoryRepository)
            => Result.FromValidation(Validate(p, categoryRepository));

        public static Result<Product> ValidateAndWrap(Product p, ICategoryRepository categoryRepository)
        {
            var errors = Validate(p, categoryRepository).ToList();
            return errors.Count == 0
                ? Result<Product>.Ok(p)
                : Result<Product>.FromErrors(errors);
        }
    }
}

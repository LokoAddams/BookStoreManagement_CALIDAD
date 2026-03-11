using System;
using System.Collections.Generic;
using System.Linq;
using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Domain.Validations
{
    public static class SalesValidation
    {
        private const int StatusMaxLength = 20;

        public static void Normalize(Sale s)
        {
            // Normalizaciones básicas
            s.Status = TextRules.NormalizeSpaces(s.Status).ToUpperInvariant();
            // Asegurar precisión monetaria a 2 decimales
            s.Subtotal = Math.Round(s.Subtotal, 2);
            s.Total = Math.Round(s.Total, 2);
        }

        public static IEnumerable<ValidationError> Validate(Sale s)
        {
            if (s.ClientId == Guid.Empty)
                yield return new ValidationError(nameof(s.ClientId), "El cliente es obligatorio.");

            if (s.UserId == Guid.Empty)
                yield return new ValidationError(nameof(s.UserId), "El usuario es obligatorio.");

            // sale_date
            if (s.SaleDate == default)
                yield return new ValidationError(nameof(s.SaleDate), "La fecha de la venta es obligatoria.");

            // subtotal
            if (s.Subtotal < 0)
                yield return new ValidationError(nameof(s.Subtotal), "El subtotal debe ser mayor o igual a 0.");

            // total
            if (s.Total < 0)
                yield return new ValidationError(nameof(s.Total), "El total debe ser mayor o igual a 0.");

            // status
            var status = TextRules.NormalizeSpaces(s.Status);
            if (string.IsNullOrWhiteSpace(status))
            {
                yield return new ValidationError(nameof(s.Status), "El estado es obligatorio.");
            }
            else if (status.Length > StatusMaxLength)
            {
                yield return new ValidationError(nameof(s.Status), $"El estado no debe superar los {StatusMaxLength} caracteres.");
            }
            else if (!IsValidStatus(status))
            {
                yield return new ValidationError(nameof(s.Status), "Estado inválido. Valores permitidos: COMPLETED, CANCELLED, PENDING, REFUNDED.");
            }

            // cancelled_by y cancelled_at coherencia
            if (s.CancelledBy.HasValue && !s.CancelledAt.HasValue)
                yield return new ValidationError(nameof(s.CancelledAt), "Debe registrar la fecha de cancelación si existe usuario que canceló.");

            if (s.CancelledAt.HasValue && !s.CancelledBy.HasValue)
                yield return new ValidationError(nameof(s.CancelledBy), "Debe registrar el usuario que canceló si existe fecha de cancelación.");

            // total = subtotal (según constraint)
            if (Math.Round(s.Total, 2) != Math.Round(s.Subtotal, 2))
                yield return new ValidationError(nameof(s.Total), "El total debe ser igual al subtotal.");

            // created_at
            if (s.CreatedAt == default)
                yield return new ValidationError(nameof(s.CreatedAt), "La fecha de creación es obligatoria.");
        }

        public static IEnumerable<ValidationError> ValidateDetail(SaleDetail d)
        {
            if (d.SaleId == Guid.Empty)
                yield return new ValidationError(nameof(d.SaleId), "La venta es obligatoria.");

            if (d.ProductId == Guid.Empty)
                yield return new ValidationError(nameof(d.ProductId), "El producto es obligatorio.");

            if (d.Quantity <= 0)
                yield return new ValidationError(nameof(d.Quantity), "La cantidad debe ser mayor a 0.");

            if (d.UnitPrice < 0)
                yield return new ValidationError(nameof(d.UnitPrice), "El precio unitario debe ser mayor o igual a 0.");

            if (d.Subtotal < 0)
                yield return new ValidationError(nameof(d.Subtotal), "El subtotal debe ser mayor o igual a 0.");

            // Consistencia subtotal = quantity * unit_price (no está en DB, pero útil)
            var expected = Math.Round(d.Quantity * d.UnitPrice, 2);
            if (Math.Round(d.Subtotal, 2) != expected)
                yield return new ValidationError(nameof(d.Subtotal), "El subtotal del detalle debe ser igual a cantidad × precio unitario.");
        }

        public static bool IsValidStatus(string status)
            => status is "COMPLETED" or "CANCELLED" or "PENDING" or "REFUNDED";

        public static IEnumerable<ValidationError> ValidateAll(Sale s)
        {
            foreach (var e in Validate(s))
                yield return e;

            if (s.Details != null)
            {
                foreach (var d in s.Details.SelectMany(ValidateDetail))
                    yield return d;

                // Unicidad por (sale_id, product_id)
                var dup = s.Details
                    .GroupBy(x => x.ProductId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .FirstOrDefault();
                if (dup != Guid.Empty)
                    yield return new ValidationError("Details", "No puede haber productos duplicados en los detalles de la venta.");
            }
        }
    }
}

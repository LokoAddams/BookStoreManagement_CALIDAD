using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicroServiceSales.Domain.Models
{
    public class Sale
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public Guid ClientId { get; set; }

        [Display(Name = "Nombre del cliente")]
        public string? ClientName { get; set; }

        [Display(Name = "CI del cliente")]
        public string? ClientCi { get; set; }

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public Guid UserId { get; set; }

        [Display(Name = "Nombre del usuario")]
        public string? UserName { get; set; }

        [Display(Name = "Fecha de venta")]
        [Required(ErrorMessage = "La fecha de la venta es obligatoria.")]
        public DateTimeOffset SaleDate { get; set; } = DateTimeOffset.UtcNow;

        [Display(Name = "Subtotal")]
        [Required(ErrorMessage = "El subtotal es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0.")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Total")]
        [Required(ErrorMessage = "El total es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0.")]
        public decimal Total { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20, ErrorMessage = "El estado no debe superar los 20 caracteres.")]
        [RegularExpression("^(COMPLETED|CANCELLED|PENDING|REFUNDED)$", ErrorMessage = "Estado inválido.")]
        public string Status { get; set; } = "COMPLETED";

        [Display(Name = "Motivo de cancelación")]
        [StringLength(500, ErrorMessage = "El motivo no debe superar los 500 caracteres.")]
        public string? CancellationReason { get; set; }

        [Display(Name = "Fecha de cancelación")]
        public DateTimeOffset? CancelledAt { get; set; }

        [Display(Name = "Cancelado por")]
        public Guid? CancelledBy { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Detalles de la venta (relación 1..n)
        public List<SaleDetail> Details { get; set; } = new List<SaleDetail>();
    }
}

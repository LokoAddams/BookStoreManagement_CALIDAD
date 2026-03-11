using System;
using System.ComponentModel.DataAnnotations;

namespace MicroServiceSales.Domain.Models
{
    public class SaleDetail
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Venta")]
        [Required(ErrorMessage = "La venta es obligatoria.")]
        public Guid SaleId { get; set; }

        [Display(Name = "Producto")]
        [Required(ErrorMessage = "El producto es obligatorio.")]
        public Guid ProductId { get; set; }

        [Display(Name = "Nombre del producto")]
        public string? ProductName { get; set; }

        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Quantity { get; set; }

        [Display(Name = "Precio unitario")]
        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0.")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Subtotal")]
        [Required(ErrorMessage = "El subtotal es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0.")]
        public decimal Subtotal { get; set; }
    }
}

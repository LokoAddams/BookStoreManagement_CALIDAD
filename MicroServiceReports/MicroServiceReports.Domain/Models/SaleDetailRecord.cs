namespace MicroServiceReports.Domain.Models
{
    using System;

    public class SaleDetailRecord
    {
        public Guid Id { get; set; }

        // Foreign key al SaleEventRecord (SaleId como string GUID)
        public string SaleId { get; set; } = string.Empty;

        // Identificador del producto
        public string ProductId { get; set; } = string.Empty;

        // Nombre del producto
        public string ProductName { get; set; } = string.Empty;

        // Cantidad vendida
        public int Quantity { get; set; }

        // Precio unitario
        public decimal UnitPrice { get; set; }

        // Subtotal (Quantity * UnitPrice)
        public decimal Subtotal { get; set; }

        // Fecha de creación del registro
        public DateTime CreatedAt { get; set; }
    }
}

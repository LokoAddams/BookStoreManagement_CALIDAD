namespace MicroServiceReports.Domain.Models
{
    using System;

    public class SaleEventRecord
    {
        public Guid Id { get; set; }

        // Identificador del sale en el sistema de ventas (GUID como string)
        public string SaleId { get; set; } = String.Empty;

        // Raw JSON payload tal como llegó
        public string Payload { get; set; } = string.Empty;

        // Metadata
        public string Exchange { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
    }
}

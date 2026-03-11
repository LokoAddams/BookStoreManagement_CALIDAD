namespace MicroServiceReports.Application.Builders
{
    /// <summary>
    /// DTO para representar un producto en el PDF
    /// </summary>
    public class ProductLineItem
    {
        public int Quantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}

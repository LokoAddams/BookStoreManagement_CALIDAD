namespace MicroServiceReports.Application.DTOs
{
    using System;
    using System.Collections.Generic;
    
    public class ProductDto
    {
        public int productId { get; set; }
        public string? name { get; set; }
        public int quantity { get; set; }
        public decimal unitPrice { get; set; }
    }

    public class SaleConfirmedEventDto
    {
        public long saleId { get; set; }
        public DateTime date { get; set; }
        public string? user { get; set; }
        public string? client { get; set; }
        public decimal total { get; set; }
        public List<ProductDto>? products { get; set; }
    }
}

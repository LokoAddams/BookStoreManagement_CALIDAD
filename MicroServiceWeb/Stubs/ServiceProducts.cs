using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceProducts.Domain.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}

namespace ServiceProducts.Domain.Interfaces
{
    using ServiceProducts.Domain.Models;
    public interface IProductRepository { IEnumerable<Product> GetAll(); }
}

namespace ServiceProducts.Application.DTOs
{
    public class ReportFilterDto
    {
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
    }

    public class GeneratedReportDto
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
}

namespace ServiceProducts.Domain.Interfaces.Reports
{
    using System.Threading;
    using System.Threading.Tasks;
    using ServiceProducts.Application.DTOs;
    public interface IProductReportService
    {
        Task<GeneratedReportDto?> GenerateAsync(ReportFilterDto filter, string format, string generatedBy, byte[]? logo, CancellationToken ct);
    }
}

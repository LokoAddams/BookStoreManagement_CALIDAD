using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceSales.Domain.Models
{
    public class SaleReportFilter
    {
        public Guid? UserId { get; set; }
        public Guid? ClientId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? StartDate { get => DateFrom; set => DateFrom = value; }
        public DateTime? EndDate { get => DateTo; set => DateTo = value; }
    }
}

namespace ServiceSales.Domain.Interfaces
{
    using System.Threading;
    using ServiceSales.Domain.Models;
    public interface ISalesReportService
    {
        Task<byte[]> GenerateSalesReportAsync(SaleReportFilter filter, string reportType, string generatedBy);
        Task<string> GetReportContentType(string reportType);
        Task<string> GetReportFileExtension(string reportType);
    }
}

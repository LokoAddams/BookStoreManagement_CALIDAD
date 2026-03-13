using ServiceSales.Domain.Interfaces;

namespace MicroServiceWeb.Stubs;

public class StubSalesReportService : ISalesReportService
{
    public Task<byte[]> GenerateSalesReportAsync(ServiceSales.Domain.Models.SaleReportFilter filter, string reportType, string generatedBy)
    {
        var content = System.Text.Encoding.UTF8.GetBytes($"Reporte ventas {reportType} generado por {generatedBy}");
        return Task.FromResult(content);
    }
    
    public Task<string> GetReportContentType(string reportType) => 
        Task.FromResult(reportType == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/pdf");
        
    public Task<string> GetReportFileExtension(string reportType) => 
        Task.FromResult(reportType == "excel" ? ".xlsx" : ".pdf");
}

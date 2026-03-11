using QuestPDF.Infrastructure;

namespace MicroServiceReports.Application.Builders
{
    /// <summary>
    /// Interfaz del Builder para construir PDFs de ventas paso a paso
    /// </summary>
    public interface ISalePdfBuilder
    {
        ISalePdfBuilder SetSaleInfo(DateTime saleDate, string client, string ci);
        ISalePdfBuilder SetUserInfo(string userName);
        ISalePdfBuilder AddProduct(int quantity, string description, decimal unitPrice);
        ISalePdfBuilder SetTotal(decimal total);
        ISalePdfBuilder SetReceivedAt(DateTime receivedAt);
        byte[] Build();
        void Reset();
    }
}

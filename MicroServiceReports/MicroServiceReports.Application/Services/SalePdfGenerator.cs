using MicroServiceReports.Application.Builders;

namespace MicroServiceReports.Application.Services
{
    /// <summary>
    /// Servicio que utiliza el patrón Builder para generar PDFs de ventas
    /// </summary>
    public class SalePdfGenerator
    {
        private readonly SalePdfDirector _director;

        public SalePdfGenerator(SalePdfDirector director)
        {
            _director = director;
        }

        /// <summary>
        /// Genera un PDF de comprobante de venta usando el patrón Builder
        /// </summary>
        public byte[] GenerateSaleReceipt(string saleJsonPayload, DateTime receivedAt)
        {
            return _director.ConstructSalePdf(saleJsonPayload, receivedAt);
        }
    }
}

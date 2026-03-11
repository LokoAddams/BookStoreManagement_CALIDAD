using MicroServiceReports.Application.Services;
using MicroServiceReports.Domain.Ports;

namespace MicroServiceReports.Application.UseCases
{
    public class GenerateSalePdfHandler
    {
        private readonly ISaleEventRepository _repository;
        private readonly SalePdfGenerator _pdfGenerator;

        public GenerateSalePdfHandler(ISaleEventRepository repository, SalePdfGenerator pdfGenerator)
        {
            _repository = repository;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<byte[]?> HandleAsync(string saleId)
        {
            var record = await _repository.GetBySaleIdAsync(saleId);
            
            if (record == null)
                return null;

            return _pdfGenerator.GenerateSaleReceipt(record.Payload, record.ReceivedAt);
        }
    }
}

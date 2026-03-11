namespace MicroServiceReports.Application.UseCases
{
    using System.Threading.Tasks;
    using MicroServiceReports.Domain.Ports;
    using MicroServiceReports.Application.DTOs;
    using MicroServiceReports.Domain.Models;

    public class GetSaleBySaleIdHandler
    {
        private readonly ISaleEventRepository _repository;

        public GetSaleBySaleIdHandler(ISaleEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<SaleEventRecord?> HandleAsync(string saleId)
        {
            return await _repository.GetBySaleIdAsync(saleId);
        }
    }
}

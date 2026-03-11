namespace MicroServiceReports.Domain.Ports
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MicroServiceReports.Domain.Models;

    public interface ISaleDetailRepository
    {
        Task SaveAsync(SaleDetailRecord record);
        Task SaveManyAsync(IEnumerable<SaleDetailRecord> records);
        Task<IEnumerable<SaleDetailRecord>> GetBySaleIdAsync(string saleId);
    }
}

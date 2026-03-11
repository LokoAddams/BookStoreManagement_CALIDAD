using MicroServiceDistributors.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceDistributors.Domain.Interfaces
{
    public interface IDistributorRepository
    {
        Task<int> CountAsync(CancellationToken ct = default);
        Task<PagedResult<Distributor>> GetPageAsync(int page, int pageSize, CancellationToken ct);
        List<Distributor> GetAll();

        Distributor? Read(Guid id);
        void Create(Distributor distributor);
        void Update(Distributor distributor);
        void Delete(Guid id);
    }
}

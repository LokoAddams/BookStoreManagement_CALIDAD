using MicroServiceClient.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceClient.Domain.Interfaces
{
    public interface IClientRepository
    {
        List<Client> GetAll();
        Task<int> CountAsync(CancellationToken ct = default);
        Task<PagedResult<Client>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Client? Read(Guid id);
        void Create(Client client);
        void Update(Client client);
        void Delete(Guid id);
        bool ExistsByCi(string ci, Guid? excludeId = null);
        Task<Client?> GetByCiAsync(string ci, CancellationToken ct = default);
    }
}

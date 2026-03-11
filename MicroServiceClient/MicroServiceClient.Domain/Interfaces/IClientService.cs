using MicroServiceClient.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceClient.Domain.Interfaces
{
    public interface IClientService
    {
        List<Client> GetAll();
        Task<PagedResult<Client>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Client? Read(Guid id);
        Task<Client?> GetByCiAsync(string ci, CancellationToken ct = default);
        void Create(Client client);
        void Update(Client client);
        void Delete(Guid id);
    }
}

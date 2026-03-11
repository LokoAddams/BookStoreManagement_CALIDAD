using MicroServiceUsers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceUsers.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllAsync(CancellationToken ct = default);
        Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
        Task CreateAsync(Role role, CancellationToken ct = default);
    }
}

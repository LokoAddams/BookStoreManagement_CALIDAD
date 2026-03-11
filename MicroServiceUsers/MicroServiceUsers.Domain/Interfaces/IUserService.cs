using MicroServiceUsers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceUsers.Domain.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync(CancellationToken ct = default);
        Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByUserOrEmailAsync(string userOrEmail, CancellationToken ct = default);
        Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct = default);
        Task CreateAsync(User user, string password, List<string> roles, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);
    }
}

using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Domain.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceUsers.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public Task<List<User>> GetAllAsync(CancellationToken ct = default) 
            => _repository.GetAllAsync(ct);

        public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
            => _repository.GetPagedAsync(page, pageSize, ct);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) 
            => _repository.GetByIdAsync(id, ct);

        public Task<User?> GetByUserOrEmailAsync(string userOrEmail, CancellationToken ct = default) 
            => _repository.GetByUserOrEmailAsync(userOrEmail, ct);

        public Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct = default) 
            => _repository.GetRolesAsync(userId, ct);

        public async Task CreateAsync(User user, string password, List<string> roles, CancellationToken ct = default)
        {
            var errors = UserValidation.Validate(user).ToList();
            if (errors.Any())
                throw new ValidationException(errors);

            UserValidation.Normalize(user);
            await _repository.CreateAsync(user, password, roles, ct);
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            var errors = UserValidation.Validate(user).ToList();
            if (errors.Any())
                throw new ValidationException(errors);

            UserValidation.Normalize(user);
            await _repository.UpdateAsync(user, ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default) 
            => _repository.DeleteAsync(id, ct);

        public Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default) 
            => _repository.ChangePasswordAsync(userId, currentPassword, newPassword, ct);
    }
}

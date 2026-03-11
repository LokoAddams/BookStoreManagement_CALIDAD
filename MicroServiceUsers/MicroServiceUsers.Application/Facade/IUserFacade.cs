using MicroServiceUsers.Application.DTOs;

namespace MicroServiceUsers.Application.Facade
{
    public interface IUserFacade
    {
        Task<UserReadDto> CreateUserAsync(UserCreateDto dto, CancellationToken ct = default);
        Task<AuthTokenDto?> LoginAsync(AuthRequestDto req, CancellationToken ct = default);
        Task<IReadOnlyList<UserReadDto>> GetAllAsync(CancellationToken ct = default);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);
    }
}

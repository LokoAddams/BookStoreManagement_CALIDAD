using MicroServiceUsers.Domain.Results;

namespace MicroServiceUsers.Domain.Interfaces
{
    public interface IJwtAuthService
    {
        Task<Result<AuthTokenData>> SignInAsync(string userOrEmail, string password, CancellationToken ct = default);
    }

    public class AuthTokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public bool MustChangePassword { get; set; }
    }
}

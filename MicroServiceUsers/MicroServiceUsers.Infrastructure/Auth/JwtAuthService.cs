using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Domain.Results;
using MicroServiceUsers.Domain.Validations;
using Microsoft.AspNetCore.Identity;

namespace MicroServiceUsers.Infrastructure.Auth
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly IUserRepository _users;
        private readonly ITokenGenerator _tokens;
        private readonly JwtOptions _options;
        private readonly PasswordHasher<User> _hasher = new();

        public JwtAuthService(IUserRepository users, ITokenGenerator tokens, JwtOptions options)
        {
            _users = users;
            _tokens = tokens;
            _options = options;
        }

        public async Task<Result<AuthTokenData>> SignInAsync(string userOrEmail, string password, CancellationToken ct = default)
        {
            var user = await _users.GetByUserOrEmailAsync(userOrEmail, ct);

            if (user is null || !user.IsActive)
                return Result<AuthTokenData>.Fail(new ValidationError("Credentials", "Credenciales inválidas."));

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, password ?? string.Empty);
            
            if (verify == PasswordVerificationResult.Failed)
                return Result<AuthTokenData>.Fail(new ValidationError("Credentials", "Credenciales inválidas."));

            // TODO: Implementar lógica para forzar cambio de contraseña en el frontend
            // if (user.MustChangePassword)
            // {
            //     return Result<AuthTokenData>.Fail(new ValidationError("MustChangePassword", "Debe cambiar su contraseña antes de continuar."));
            // }

            // Obtener roles del usuario desde la base de datos
            var roles = await _users.GetRolesAsync(user.Id, ct);
            if (!roles.Any())
                roles = new List<string> { "User" };

            var now = DateTimeOffset.UtcNow;
            var jwt = _tokens.CreateToken(user, roles, now, _options);

            var tokenData = new AuthTokenData
            {
                AccessToken = jwt,
                ExpiresAt = now.AddMinutes(_options.ExpiresMinutes),
                UserName = user.Username,
                Roles = roles.ToArray(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                MustChangePassword = user.MustChangePassword
            };

            return Result<AuthTokenData>.Ok(tokenData);
        }
    }
}

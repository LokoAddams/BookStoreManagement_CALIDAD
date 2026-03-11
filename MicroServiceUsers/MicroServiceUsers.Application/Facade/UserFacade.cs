using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MicroServiceUsers.Application.DTOs;
using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;

namespace MicroServiceUsers.Application.Facade
{
    public class UserFacade : IUserFacade
    {
        private readonly IUserService _users;
        private readonly IJwtAuthService _auth;
        private readonly IPasswordGenerator _pwdGen;
        private readonly IUsernameGenerator _unameGen;
        private readonly IEmailService _email;
        private readonly ILogger<UserFacade> _logger;

        public UserFacade(
            IUserService users,
            IJwtAuthService auth,
            IPasswordGenerator pwdGen,
            IUsernameGenerator unameGen,
            IEmailService email,
            ILogger<UserFacade> logger)
        {
            _users = users;
            _auth = auth;
            _pwdGen = pwdGen;
            _unameGen = unameGen;
            _email = email;
            _logger = logger;
        }

        public async Task<UserReadDto> CreateUserAsync(UserCreateDto dto, CancellationToken ct = default)
        {
            var baseUsername = _unameGen.GenerateUsernameFromEmail(dto.Email);
            var allUsers = await _users.GetAllAsync(ct);
            var uniqueUsername = _unameGen.EnsureUniqueUsername(
                baseUsername,
                u => allUsers.Any(x => x.Username.Equals(u, StringComparison.OrdinalIgnoreCase)));

            var plainPassword = _pwdGen.GenerateSecurePassword();

            var user = new User
            {
                Email = dto.Email.Trim().ToLowerInvariant(),
                Username = uniqueUsername,
                FirstName = string.Empty,
                LastName = string.Empty,
                PasswordHash = string.Empty, // Se establecerá en el repositorio
                IsActive = true,
                MustChangePassword = true
            };

            var roles = new List<string> { dto.Role };
            await _users.CreateAsync(user, plainPassword, roles, ct);

            _logger.LogInformation("Usuario creado: {Username}, Email: {Email}. Enviando correo con credenciales...", 
                uniqueUsername, dto.Email);

            // Enviar email con credenciales
            var html = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #333;'>Bienvenido a Bookstore Management System</h2>
                    <p>Se ha creado una cuenta para ti. A continuación tus credenciales de acceso:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Usuario:</strong> {uniqueUsername}</p>
                        <p><strong>Contraseña:</strong> {plainPassword}</p>
                        <p><strong>Rol:</strong> {dto.Role}</p>
                    </div>
                    <p style='color: #ff6600;'><strong>Importante:</strong> Por seguridad, te recomendamos cambiar tu contraseña después del primer inicio de sesión.</p>
                    <p>Saludos,<br>El equipo de Bookstore Management System</p>
                </body>
                </html>";

            var emailSent = await _email.SendEmailAsync(dto.Email, "Credenciales de acceso - Bookstore Management System", html, ct);
            
            if (emailSent)
            {
                _logger.LogInformation("Correo enviado exitosamente a {Email}", dto.Email);
            }
            else
            {
                _logger.LogWarning("No se pudo enviar el correo a {Email}. El usuario fue creado pero no recibió las credenciales.", dto.Email);
            }

            return new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = new[] { dto.Role }
            };
        }

        public async Task<AuthTokenDto?> LoginAsync(AuthRequestDto req, CancellationToken ct = default)
        {
            var result = await _auth.SignInAsync(req.UserOrEmail, req.Password, ct);
            
            if (!result.IsSuccess || result.Value is null)
                return null;

            // Convertir AuthTokenData a AuthTokenDto
            var data = result.Value;
            return new AuthTokenDto
            {
                AccessToken = data.AccessToken,
                ExpiresAt = data.ExpiresAt,
                UserName = data.UserName,
                Roles = data.Roles,
                Email = data.Email,
                FirstName = data.FirstName,
                LastName = data.LastName,
                MiddleName = data.MiddleName,
                MustChangePassword = data.MustChangePassword
            };
        }

        public async Task<IReadOnlyList<UserReadDto>> GetAllAsync(CancellationToken ct = default)
        {
            var allUsers = await _users.GetAllAsync(ct);
            var result = new List<UserReadDto>();

            foreach (var u in allUsers)
            {
                var roles = await _users.GetRolesAsync(u.Id, ct);
                result.Add(new UserReadDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Roles = roles.ToArray()
                });
            }

            return result.AsReadOnly();
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            return await _users.ChangePasswordAsync(userId, currentPassword, newPassword, ct);
        }
    }
}

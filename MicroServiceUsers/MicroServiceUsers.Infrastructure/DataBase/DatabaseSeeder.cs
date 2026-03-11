using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MicroServiceUsers.Infrastructure.DataBase
{
    public class DatabaseSeeder
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DatabaseSeeder>? _logger;

        public DatabaseSeeder(IUserRepository userRepository, ILogger<DatabaseSeeder>? logger = null)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            try
            {
                // Verificar si ya existe el usuario admin
                var adminUser = await _userRepository.GetByUserOrEmailAsync("admin@admin.com", ct);
                
                if (adminUser != null)
                {
                    _logger?.LogInformation("Usuario admin ya existe, saltando seed.");
                    return;
                }

                _logger?.LogInformation("Creando usuario administrador por defecto...");

                var admin = new User
                {
                    Username = "admin",
                    Email = "admin@admin.com",
                    FirstName = "Admin",
                    LastName = "System",
                    MiddleName = string.Empty,
                    PasswordHash = string.Empty,
                    IsActive = true,
                    MustChangePassword = false
                };

                var roles = new List<string> { "Admin" };
                await _userRepository.CreateAsync(admin, "admin123", roles, ct);

                _logger?.LogInformation("Usuario administrador creado exitosamente: admin@admin.com / admin123");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al realizar seed de la base de datos: {Message}", ex.Message);
            }
        }
    }
}

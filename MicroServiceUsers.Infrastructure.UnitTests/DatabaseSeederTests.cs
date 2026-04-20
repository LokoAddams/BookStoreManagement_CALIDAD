using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Infrastructure.DataBase;
using Microsoft.Extensions.Logging;

namespace MicroServiceUsers.Infrastructure.UnitTests;

public class DatabaseSeederTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task SeedAsync_Scenarios_BehaveAsExpected(int testCase)
    {
        var repository = new FakeUserRepository();
        var logger = new FakeLogger<DatabaseSeeder>();
        var sut = new DatabaseSeeder(repository, logger);

        switch (testCase)
        {
            case 1:
                repository.GetByUserOrEmailResult = new User { Id = Guid.NewGuid(), Email = "admin@admin.com" };
                break;
            case 2:
                repository.GetByUserOrEmailResult = null;
                break;
            case 3:
                repository.ThrowOnGetByUserOrEmail = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(testCase), testCase, "Caso no soportado");
        }

        var exception = await Record.ExceptionAsync(() => sut.SeedAsync(CancellationToken.None));

        Assert.Null(exception);

        if (testCase == 1)
        {
            Assert.Equal(0, repository.CreateCalls);
            Assert.Contains(logger.Logs, l => l.Level == LogLevel.Information && l.Message.Contains("Usuario admin ya existe"));
            return;
        }

        if (testCase == 2)
        {
            Assert.Equal(1, repository.CreateCalls);
            Assert.NotNull(repository.LastCreatedUser);
            Assert.Equal("admin", repository.LastCreatedUser!.Username);
            Assert.Equal("admin@admin.com", repository.LastCreatedUser.Email);
            Assert.Equal("Admin", repository.LastCreatedUser.FirstName);
            Assert.Equal("System", repository.LastCreatedUser.LastName);
            Assert.Equal("admin123", repository.LastCreatedPassword);
            Assert.Single(repository.LastCreatedRoles);
            Assert.Equal("Admin", repository.LastCreatedRoles[0]);

            Assert.Contains(logger.Logs, l => l.Level == LogLevel.Information && l.Message.Contains("Creando usuario administrador por defecto"));
            Assert.Contains(logger.Logs, l => l.Level == LogLevel.Information && l.Message.Contains("Usuario administrador creado exitosamente"));
            return;
        }

        Assert.Equal(0, repository.CreateCalls);
        Assert.Contains(logger.Logs, l => l.Level == LogLevel.Error && l.Message.Contains("Error al realizar seed de la base de datos"));
        Assert.Contains(logger.Logs, l => l.Level == LogLevel.Error && l.Exception is not null);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? GetByUserOrEmailResult { get; set; }
        public bool ThrowOnGetByUserOrEmail { get; set; }

        public int CreateCalls { get; private set; }
        public User? LastCreatedUser { get; private set; }
        public string LastCreatedPassword { get; private set; } = string.Empty;
        public List<string> LastCreatedRoles { get; private set; } = [];

        public Task<User?> GetByUserOrEmailAsync(string userOrEmail, CancellationToken ct = default)
        {
            if (ThrowOnGetByUserOrEmail)
                throw new Exception("DB connection failed");

            return Task.FromResult(GetByUserOrEmailResult);
        }

        public Task CreateAsync(User user, string password, List<string> roles, CancellationToken ct = default)
        {
            CreateCalls++;
            LastCreatedUser = user;
            LastCreatedPassword = password;
            LastCreatedRoles = roles;
            return Task.CompletedTask;
        }

        public Task<List<User>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<int> CountAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task UpdateAsync(User user, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Logs { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Logs.Add((logLevel, formatter(state, exception), exception));
        }
    }
}

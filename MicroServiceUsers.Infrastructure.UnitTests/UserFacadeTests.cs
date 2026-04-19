using Microsoft.Extensions.Logging;
using MicroServiceUsers.Application.DTOs;
using MicroServiceUsers.Application.Facade;
using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Domain.Results;
using MicroServiceUsers.Domain.Validations;

namespace MicroServiceUsers.Infrastructure.UnitTests;

public class UserFacadeTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task LoginAsync_Scenarios_ReturnExpectedResult(int testCase)
    {
        var users = new FakeUserService();
        var auth = new FakeJwtAuthService();
        var passwordGenerator = new FakePasswordGenerator("Pwd#123");
        var usernameGenerator = new FakeUsernameGenerator("newuser", "newuser");
        var email = new FakeEmailService(true);
        var logger = new FakeLogger<UserFacade>();
        var sut = new UserFacade(users, auth, passwordGenerator, usernameGenerator, email, logger);

        var request = new AuthRequestDto
        {
            UserOrEmail = "test@bookstore.com",
            Password = "pass123"
        };

        switch (testCase)
        {
            case 1:
                auth.SignInResult = Result<AuthTokenData>.Fail(new ValidationError("Credentials", "Credenciales inválidas."));
                break;
            case 2:
                auth.SignInResult = Result<AuthTokenData>.Ok(null!);
                break;
            case 3:
                auth.SignInResult = Result<AuthTokenData>.Ok(new AuthTokenData
                {
                    AccessToken = "jwt-token",
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60),
                    UserName = "jdoe",
                    Roles = ["Admin"],
                    Email = "test@bookstore.com",
                    FirstName = "John",
                    LastName = "Doe",
                    MiddleName = "M",
                    MustChangePassword = false
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(testCase), testCase, "Caso no soportado");
        }

        var result = await sut.LoginAsync(request, CancellationToken.None);

        if (testCase is 1 or 2)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal("jwt-token", result!.AccessToken);
            Assert.Equal("jdoe", result.UserName);
            Assert.Equal("test@bookstore.com", result.Email);
            Assert.Single(result.Roles);
            Assert.Equal("Admin", result.Roles[0]);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("M", result.MiddleName);
            Assert.False(result.MustChangePassword);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ChangePasswordAsync_ReturnsSameValueAndCallsServiceOnceWithExactParameters(bool expectedResult)
    {
        var users = new FakeUserService
        {
            ChangePasswordResult = expectedResult
        };
        var auth = new FakeJwtAuthService();
        var passwordGenerator = new FakePasswordGenerator("Pwd#123");
        var usernameGenerator = new FakeUsernameGenerator("newuser", "newuser");
        var email = new FakeEmailService(true);
        var logger = new FakeLogger<UserFacade>();
        var sut = new UserFacade(users, auth, passwordGenerator, usernameGenerator, email, logger);

        var userId = Guid.NewGuid();
        var current = "pass123";
        var next = "pass456";

        var result = await sut.ChangePasswordAsync(userId, current, next, CancellationToken.None);

        Assert.Equal(expectedResult, result);
        Assert.Equal(1, users.ChangePasswordCalls);
        Assert.Equal(userId, users.LastChangePasswordUserId);
        Assert.Equal(current, users.LastChangePasswordCurrentPassword);
        Assert.Equal(next, users.LastChangePasswordNewPassword);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateUserAsync_WhenEmailSendingSucceedsOrFails_ReturnsDtoAndLogsExpectedLevel(bool emailSent)
    {
        var users = new FakeUserService
        {
            AllUsers = []
        };
        var auth = new FakeJwtAuthService();
        var passwordGenerator = new FakePasswordGenerator("Pwd#123");
        var usernameGenerator = new FakeUsernameGenerator("newuser", "newuser");
        var email = new FakeEmailService(emailSent);
        var logger = new FakeLogger<UserFacade>();

        var sut = new UserFacade(users, auth, passwordGenerator, usernameGenerator, email, logger);

        var dto = new UserCreateDto { Email = "test@bookstore.com", Role = "Admin" };

        var result = await sut.CreateUserAsync(dto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("test@bookstore.com", result.Email);
        Assert.Single(result.Roles);
        Assert.Equal("Admin", result.Roles[0]);

        Assert.Equal(1, users.CreateCalls);

        if (emailSent)
        {
            Assert.Contains(logger.Logs, l => l.Level == LogLevel.Information && l.Message.Contains("Correo enviado exitosamente"));
        }
        else
        {
            Assert.Contains(logger.Logs, l => l.Level == LogLevel.Warning && l.Message.Contains("No se pudo enviar el correo"));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetAllAsync_WhenUsersExistOrNot_ReturnsExpectedListAndCallsGetRolesExpectedTimes(bool withUsers)
    {
        var users = new FakeUserService();
        if (withUsers)
        {
            users.AllUsers =
            [
                new User { Id = Guid.NewGuid(), Username = "u1", Email = "u1@test.com" },
                new User { Id = Guid.NewGuid(), Username = "u2", Email = "u2@test.com" }
            ];
        }
        else
        {
            users.AllUsers = [];
        }

        var auth = new FakeJwtAuthService();
        var passwordGenerator = new FakePasswordGenerator("Pwd#123");
        var usernameGenerator = new FakeUsernameGenerator("newuser", "newuser");
        var email = new FakeEmailService(true);
        var logger = new FakeLogger<UserFacade>();
        var sut = new UserFacade(users, auth, passwordGenerator, usernameGenerator, email, logger);

        var result = await sut.GetAllAsync(CancellationToken.None);

        if (withUsers)
        {
            Assert.Equal(2, result.Count);
            Assert.Equal(2, users.GetRolesCalls);
        }
        else
        {
            Assert.Empty(result);
            Assert.Equal(0, users.GetRolesCalls);
        }
    }

    private sealed class FakeUserService : IUserService
    {
        public List<User> AllUsers { get; set; } = [];
        public int CreateCalls { get; private set; }
        public int GetRolesCalls { get; private set; }
        public bool ChangePasswordResult { get; set; }
        public int ChangePasswordCalls { get; private set; }
        public Guid LastChangePasswordUserId { get; private set; }
        public string LastChangePasswordCurrentPassword { get; private set; } = string.Empty;
        public string LastChangePasswordNewPassword { get; private set; } = string.Empty;

        public Task<List<User>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(AllUsers);

        public Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
        {
            GetRolesCalls++;
            return Task.FromResult(new List<string> { "User" });
        }

        public Task CreateAsync(User user, string password, List<string> roles, CancellationToken ct = default)
        {
            CreateCalls++;
            user.Id = Guid.NewGuid();
            return Task.CompletedTask;
        }

        public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<User?> GetByUserOrEmailAsync(string userOrEmail, CancellationToken ct = default) => throw new NotImplementedException();
        public Task UpdateAsync(User user, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            ChangePasswordCalls++;
            LastChangePasswordUserId = userId;
            LastChangePasswordCurrentPassword = currentPassword;
            LastChangePasswordNewPassword = newPassword;
            return Task.FromResult(ChangePasswordResult);
        }
    }

    private sealed class FakeJwtAuthService : IJwtAuthService
    {
        public Result<AuthTokenData> SignInResult { get; set; } = Result<AuthTokenData>.Fail();

        public Task<Result<AuthTokenData>> SignInAsync(string userOrEmail, string password, CancellationToken ct = default)
            => Task.FromResult(SignInResult);
    }

    private sealed class FakePasswordGenerator(string password) : IPasswordGenerator
    {
        public string GenerateSecurePassword() => password;
    }

    private sealed class FakeUsernameGenerator(string generatedBase, string unique) : IUsernameGenerator
    {
        public string GenerateUsernameFromEmail(string email) => generatedBase;
        public string EnsureUniqueUsername(string baseUsername, Func<string, bool> existsCheck) => unique;
    }

    private sealed class FakeEmailService(bool result) : IEmailService
    {
        public Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken ct = default)
            => Task.FromResult(result);
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Logs.Add((logLevel, formatter(state, exception)));
        }
    }
}

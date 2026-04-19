using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;

namespace MicroServiceUsers.Infrastructure.UnitTests;

public class JwtAuthServiceTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task SignInAsync_Scenarios_ReturnExpectedResult(int testCase)
    {
        var now = DateTimeOffset.UtcNow;
        var options = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "this-is-a-test-key-with-enough-length-123456",
            ExpiresMinutes = 60
        };

        var tokenGenerator = new FakeTokenGenerator();
        var repository = new FakeUserRepository();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "jdoe",
            Email = "jdoe@test.com",
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "M",
            IsActive = true,
            MustChangePassword = false
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, "Correct#123");

        string userOrEmail = "jdoe@test.com";
        string? password = "Correct#123";

        switch (testCase)
        {
            case 1: // TC1 Happy Path: rol Admin
                repository.UserByCredentials = user;
                repository.Roles = ["Admin"];
                break;

            case 2: // TC2 user null
                repository.UserByCredentials = null;
                break;

            case 3: // TC3 user inactivo
                user.IsActive = false;
                repository.UserByCredentials = user;
                break;

            case 4: // TC4 password null -> usa string.Empty internamente
                repository.UserByCredentials = user;
                password = null;
                break;

            case 5: // TC5 password incorrecto
                repository.UserByCredentials = user;
                password = "Wrong#123";
                break;

            case 6: // TC6 roles vacíos -> rol por defecto User
                repository.UserByCredentials = user;
                repository.Roles = [];
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(testCase), testCase, "Caso de prueba no soportado");
        }

        var sut = new JwtAuthService(repository, tokenGenerator, options);

        var result = await sut.SignInAsync(userOrEmail, password!, CancellationToken.None);

        if (testCase is 1 or 6)
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("fake-jwt", result.Value!.AccessToken);

            if (testCase == 1)
            {
                Assert.Contains("Admin", result.Value.Roles);
                Assert.Contains("Admin", tokenGenerator.LastRoles);
            }
            else
            {
                Assert.Single(result.Value.Roles);
                Assert.Equal("User", result.Value.Roles[0]);
                Assert.Single(tokenGenerator.LastRoles);
                Assert.Equal("User", tokenGenerator.LastRoles[0]);
            }
        }
        else
        {
            Assert.True(result.IsFailure);
            Assert.Contains(result.Errors, e => e.Field == "Credentials" && e.Message == "Credenciales inválidas.");
            Assert.Equal(0, tokenGenerator.CreateTokenCalls);
        }
    }

    private sealed class FakeTokenGenerator : ITokenGenerator
    {
        public int CreateTokenCalls { get; private set; }
        public string[] LastRoles { get; private set; } = [];

        public string CreateToken(User user, IEnumerable<string> roles, DateTimeOffset now, object options)
        {
            CreateTokenCalls++;
            LastRoles = roles.ToArray();
            return "fake-jwt";
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? UserByCredentials { get; set; }
        public List<string> Roles { get; set; } = ["Admin"];

        public Task<User?> GetByUserOrEmailAsync(string userOrEmail, CancellationToken ct = default)
            => Task.FromResult(UserByCredentials);

        public Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(Roles);

        public Task<List<User>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<int> CountAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task CreateAsync(User user, string password, List<string> roles, CancellationToken ct = default) => throw new NotImplementedException();
        public Task UpdateAsync(User user, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default) => throw new NotImplementedException();
    }
}

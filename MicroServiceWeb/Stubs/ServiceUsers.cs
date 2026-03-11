using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceUsers.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

namespace ServiceUsers.Domain.Interfaces
{
    using ServiceUsers.Application.DTOs;
    using ServiceUsers.Domain.Models;

    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User? Read(Guid id);
        void Update(User user);
        void Delete(Guid id);
        List<string> GetUserRoles(Guid id);
        void UpdateUserRoles(Guid id, List<string> roles);
    }

    public interface IJwtAuthService
    {
        Task<SignInResult> SignInAsync(AuthRequestDto request, CancellationToken ct);
    }
}

namespace ServiceUsers.Application.DTOs
{
    using System;
    using System.Collections.Generic;

    public class AuthRequestDto
    {
        public string UserOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddHours(1);
        public bool MustChangePassword { get; set; } // agregado para flujo de primer inicio
    }

    public class UserCreateDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class SignInResult
    {
        public bool IsSuccess => Value != null && Errors.Count == 0;
        public AuthResponseDto? Value { get; set; }
        public List<AuthError> Errors { get; set; } = new();
    }
    public class AuthError { public string Field { get; set; } = string.Empty; public string Message { get; set; } = string.Empty; }
}

namespace ServiceUsers.Application.Facade
{
    using System.Threading;
    using System.Threading.Tasks;
    using ServiceUsers.Application.DTOs;

    public interface IUserFacade
    {
        Task CreateUserAsync(UserCreateDto dto, CancellationToken ct);
        Task<AuthResponseDto?> LoginAsync(AuthRequestDto dto, CancellationToken ct);
    }
}

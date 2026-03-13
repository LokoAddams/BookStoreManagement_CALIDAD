using ServiceUsers.Domain.Interfaces;

namespace MicroServiceWeb.Stubs;

public class StubJwtAuthService : IJwtAuthService
{
    private readonly IUserService _userService;
    
    public StubJwtAuthService(IUserService userService) 
    { 
        _userService = userService; 
    }
    
    public Task<ServiceUsers.Application.DTOs.SignInResult> SignInAsync(ServiceUsers.Application.DTOs.AuthRequestDto request, CancellationToken ct)
    {
        var usr = _userService.GetAll().FirstOrDefault(u => u.Username.Equals(request.UserOrEmail, StringComparison.OrdinalIgnoreCase));
        var result = new ServiceUsers.Application.DTOs.SignInResult();
        
        if (usr == null || usr.PasswordHash != request.Password)
        {
            result.Errors.Add(new ServiceUsers.Application.DTOs.AuthError { Field = "Credentials", Message = "Credenciales inválidas" });
            return Task.FromResult(result);
        }
        
        result.Value = new ServiceUsers.Application.DTOs.AuthResponseDto
        {
            UserName = usr.Username,
            Email = usr.Email,
            FirstName = usr.FirstName,
            MiddleName = usr.MiddleName,
            LastName = usr.LastName,
            Roles = usr.Roles,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(8),
            MustChangePassword = usr.MustChangePassword
        };
        
        if (usr.MustChangePassword)
            result.Errors.Add(new ServiceUsers.Application.DTOs.AuthError { Field = "MustChangePassword", Message = "Debe cambiar contraseña" });
            
        return Task.FromResult(result);
    }
}

using ServiceUsers.Application.Facade;
using ServiceUsers.Domain.Interfaces;

namespace MicroServiceWeb.Stubs;

public class StubUserFacade : IUserFacade
{
    private readonly IUserService _userService;
    public StubUserFacade(IUserService userService) { _userService = userService; }
    
    public Task CreateUserAsync(ServiceUsers.Application.DTOs.UserCreateDto dto, CancellationToken ct)
    { 
        return Task.CompletedTask; 
    }
    
    public Task<ServiceUsers.Application.DTOs.AuthResponseDto?> LoginAsync(ServiceUsers.Application.DTOs.AuthRequestDto dto, CancellationToken ct)
    { 
        return Task.FromResult<ServiceUsers.Application.DTOs.AuthResponseDto?>(null); 
    }
}

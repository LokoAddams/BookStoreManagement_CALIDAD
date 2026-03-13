using ServiceUsers.Domain.Interfaces;

namespace MicroServiceWeb.Stubs;

public class StubUserService : IUserService
{
    private readonly List<ServiceUsers.Domain.Models.User> _users = new();
    
    public StubUserService()
    {
        // Predefinido: deben cambiar contraseña en primer login
        _users.Add(new ServiceUsers.Domain.Models.User { Username = "admin", Email = "admin@test.local", Roles = new List<string>{"Admin"}, PasswordHash = "admin", MustChangePassword = true });
        _users.Add(new ServiceUsers.Domain.Models.User { Username = "empleado", Email = "empleado@test.local", Roles = new List<string>{"Employee"}, PasswordHash = "empleado", MustChangePassword = true });
    }
    
    public IEnumerable<ServiceUsers.Domain.Models.User> GetAll() => _users;
    public ServiceUsers.Domain.Models.User? Read(Guid id) => _users.FirstOrDefault(u => u.Id == id);
    public void Update(ServiceUsers.Domain.Models.User user) { }
    public void Delete(Guid id) { _users.RemoveAll(u => u.Id == id); }
    public List<string> GetUserRoles(Guid id) => _users.FirstOrDefault(u => u.Id == id)?.Roles ?? new List<string>();
    
    public void UpdateUserRoles(Guid id, List<string> roles)
    {
        var u = _users.FirstOrDefault(x => x.Id == id);
        if (u != null) u.Roles = roles;
    }
}

using MicroServiceUsers.Domain.Models;

namespace MicroServiceUsers.Domain.Interfaces
{
    public interface ITokenGenerator
    {
        string CreateToken(User user, IEnumerable<string> roles, DateTimeOffset now, object options);
    }
}

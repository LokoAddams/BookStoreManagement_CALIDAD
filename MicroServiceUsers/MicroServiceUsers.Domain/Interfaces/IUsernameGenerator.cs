namespace MicroServiceUsers.Domain.Interfaces
{
    public interface IUsernameGenerator
    {
        string GenerateUsernameFromEmail(string email);
        string EnsureUniqueUsername(string baseUsername, Func<string, bool> existsCheck);
    }
}

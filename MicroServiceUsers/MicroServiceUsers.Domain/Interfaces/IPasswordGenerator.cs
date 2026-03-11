namespace MicroServiceUsers.Domain.Interfaces
{
    public interface IPasswordGenerator
    {
        string GenerateSecurePassword();
    }
}

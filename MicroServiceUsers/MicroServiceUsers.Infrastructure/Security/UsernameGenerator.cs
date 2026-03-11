using MicroServiceUsers.Domain.Interfaces;

namespace MicroServiceUsers.Infrastructure.Security
{
    public class UsernameGenerator : IUsernameGenerator
    {
        public string GenerateUsernameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var localPart = email.Split('@')[0];
            
            return localPart.ToLowerInvariant().Trim();
        }

        public string EnsureUniqueUsername(string baseUsername, Func<string, bool> existsCheck)
        {
            if (string.IsNullOrWhiteSpace(baseUsername))
                throw new ArgumentException("Base username cannot be null or empty", nameof(baseUsername));

            var username = baseUsername;
            var suffix = 1;

            while (existsCheck(username))
            {
                username = $"{baseUsername}{suffix}";
                suffix++;
                
                if (suffix > 9999)
                    throw new InvalidOperationException("Could not generate unique username");
            }

            return username;
        }
    }
}

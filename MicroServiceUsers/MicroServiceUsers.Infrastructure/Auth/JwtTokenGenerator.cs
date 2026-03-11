using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace MicroServiceUsers.Infrastructure.Auth
{
    public class JwtTokenGenerator : ITokenGenerator
    {
        public string CreateToken(User user, IEnumerable<string> roles, DateTimeOffset now, object options)
        {
            if (options is not JwtOptions jwtOptions)
                throw new ArgumentException("Invalid options type", nameof(options));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Name, user.Username),
                new("given_name", user.FirstName ?? string.Empty),
                new("family_name", user.LastName ?? string.Empty),
                new("middle_name", user.MiddleName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: now.AddMinutes(jwtOptions.ExpiresMinutes).UtcDateTime,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

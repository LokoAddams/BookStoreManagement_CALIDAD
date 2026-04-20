using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Infrastructure.Auth;

namespace MicroServiceUsers.Infrastructure.UnitTests;

public class JwtTokenGeneratorTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateToken_Scenarios_ReturnExpectedResult(int testCase)
    {
        var sut = new JwtTokenGenerator();
        var now = DateTimeOffset.UtcNow;

        var validOptions = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "this-is-a-test-key-with-enough-length-123456",
            ExpiresMinutes = 60
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "jdoe",
            Email = "jdoe@test.com",
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Mark"
        };

        IEnumerable<string> roles = ["Admin"];
        object options = validOptions;

        if (testCase == 2)
        {
            options = new object();
        }
        else if (testCase == 3)
        {
            user.FirstName = null;
            user.LastName = null;
            user.MiddleName = null;
            roles = [];
        }

        if (testCase == 2)
        {
            var ex = Assert.Throws<ArgumentException>(() => sut.CreateToken(user, roles, now, options));
            Assert.Equal("options", ex.ParamName);
            return;
        }

        var token = sut.CreateToken(user, roles, now, options);

        Assert.False(string.IsNullOrWhiteSpace(token));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(validOptions.Issuer, jwt.Issuer);
        Assert.Contains(validOptions.Audience, jwt.Audiences);

        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Username, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);

        var givenName = jwt.Claims.First(c => c.Type == "given_name").Value;
        var familyName = jwt.Claims.First(c => c.Type == "family_name").Value;
        var middleName = jwt.Claims.First(c => c.Type == "middle_name").Value;

        if (testCase == 1)
        {
            Assert.Equal("John", givenName);
            Assert.Equal("Doe", familyName);
            Assert.Equal("Mark", middleName);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }
        else
        {
            Assert.Equal(string.Empty, givenName);
            Assert.Equal(string.Empty, familyName);
            Assert.Equal(string.Empty, middleName);
            Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.Role);
        }
    }
}

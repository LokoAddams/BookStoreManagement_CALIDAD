using System.Reflection;
using MicroServiceUsers.Infrastructure.Security;

namespace MicroServiceUsers.Infrastructure.UnitTests;

public class SecurePasswordGeneratorTests
{
    [Theory]
    [InlineData("^[A-Z][a-z]+[0-9]{2}[A-Z][a-z]+[0-9]{2}$", 10)]
    public void GenerateSecurePassword_Scenarios_ReturnExpectedResult(string pattern, int minLength)
    {
        var sut = new SecurePasswordGenerator();

        var result = sut.GenerateSecurePassword();

        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Matches(pattern, result);
        Assert.True(result.Length >= minLength);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("luna", "Luna")]
    public void CapitalizeFirst_Scenarios_ReturnExpectedResult(string? word, string? expected)
    {
        var sut = new SecurePasswordGenerator();
        var method = typeof(SecurePasswordGenerator).GetMethod("CapitalizeFirst", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var result = (string?)method.Invoke(sut, [word]);

        Assert.Equal(expected, result);
    }
}

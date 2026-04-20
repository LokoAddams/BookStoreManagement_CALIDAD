using MicroServiceUsers.Infrastructure.Security;

namespace MicroServiceUsers.Infrastructure.UnitTests;

public class UsernameGeneratorTests
{
    [Theory]
    [InlineData(1, null, null)]
    [InlineData(2, "   ", null)]
    [InlineData(3, " LuCas.ALcoba@ucb.edu.bo ", "lucas.alcoba")]
    public void GenerateUsernameFromEmail_Scenarios_ReturnExpectedResult(int testCase, string? email, string? expectedUsername)
    {
        var sut = new UsernameGenerator();

        if (testCase is 1 or 2)
        {
            Assert.Throws<ArgumentException>(() => sut.GenerateUsernameFromEmail(email!));
            return;
        }

        var result = sut.GenerateUsernameFromEmail(email!);
        Assert.Equal(expectedUsername, result);
    }

    [Theory]
    [InlineData(1, null, null)]
    [InlineData(2, "", null)]
    [InlineData(3, "lucas.alcoba", "lucas.alcoba")]
    [InlineData(4, "lucas.alcoba", "lucas.alcoba1")]
    [InlineData(5, "lucas.alcoba", null)]
    public void EnsureUniqueUsername_Scenarios_ReturnExpectedResult(int testCase, string? baseUsername, string? expectedUsername)
    {
        var sut = new UsernameGenerator();

        if (testCase is 1 or 2)
        {
            Assert.Throws<ArgumentException>(() => sut.EnsureUniqueUsername(baseUsername!, _ => false));
            return;
        }

        if (testCase == 5)
        {
            Assert.Throws<InvalidOperationException>(() => sut.EnsureUniqueUsername(baseUsername!, _ => true));
            return;
        }

        Func<string, bool> existsCheck = testCase switch
        {
            3 => _ => false,
            4 => CreateExistsOnceMock(),
            _ => _ => false
        };

        var result = sut.EnsureUniqueUsername(baseUsername!, existsCheck);
        Assert.Equal(expectedUsername, result);
    }

    private static Func<string, bool> CreateExistsOnceMock()
    {
        var callCount = 0;
        return _ =>
        {
            callCount++;
            return callCount == 1;
        };
    }
}

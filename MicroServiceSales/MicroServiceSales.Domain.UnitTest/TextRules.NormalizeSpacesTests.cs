using MicroServiceSales.Domain.Validations;
using Xunit;

namespace MicroServiceSales.Domain.UnitTest;

public class TextRulesNormalizeSpacesTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData(" ", "")]
    [InlineData("   ", "")]
    [InlineData("Hola   Mundo", "Hola Mundo")]
    [InlineData("  Hola    Mundo  ", "Hola Mundo")]
    public void NormalizeSpaces_Should_Return_Normalized_Text(string? input, string expected)
    {
        var result = TextRules.NormalizeSpaces(input);

        Assert.Equal(expected, result);
    }
}

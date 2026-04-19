using MicroServiceDistributors.Domain.Validations;
using Xunit;

namespace MicroServiceDistributors.Domain.UnitTest
{
    public class TextRulesTests
    {
        [Theory]
        [InlineData("   hola      mundo   ", false, "hola mundo")]
        [InlineData(null, true, "")]
        public void NormalizeSpaces_ShouldReturnExpectedValue(string? s, bool isNullOrWhiteSpace, string expected)
        {
            // Act
            var result = TextRules.NormalizeSpaces(s);

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(isNullOrWhiteSpace, string.IsNullOrWhiteSpace(s));
        }

        [Theory]
        [InlineData("  distribuidora   SUR  ", false, "Distribuidora SUR")]
        [InlineData("   ", true, "")]
        [InlineData("a", false, "A")]
        public void CanonicalSentence_ShouldReturnExpectedValue(string? s, bool isEmptyAfterNormalize, string expected)
        {
            // Act
            var result = TextRules.CanonicalSentence(s);

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(isEmptyAfterNormalize, string.IsNullOrEmpty(TextRules.NormalizeSpaces(s)));
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("ventas@empresa.com", true)]
        public void IsValidEmail_ShouldReturnExpectedValue(string? s, bool expected)
        {
            // Act
            var result = TextRules.IsValidEmail(s);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("   ", false)]
        [InlineData("Teclado Mecánico, 100 - Blanco.", true)]
        public void IsValidProductDescriptionLoose_ShouldReturnExpectedValue(string? s, bool expected)
        {
            // Act
            var result = TextRules.IsValidProductDescriptionLoose(s);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

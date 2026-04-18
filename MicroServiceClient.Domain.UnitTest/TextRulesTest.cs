using Xunit;
using System.Linq;
using System.Collections.Generic;
using MicroServiceClient.Domain.Validations;

namespace MicroServiceClient.Domain.UnitTest
{
    public class TextRulesTest
    {

        [Theory]
        [InlineData(null, "")]             // TC1: Caso nulo
        [InlineData("   ", "")]             // TC1: Caso solo espacios
        [InlineData("  hola   mundo  ", "hola mundo")] // TC2: Caso con espacios múltiples
        public void NormalizeSpaces_PathCoverage_Tests(string input, string expected)
        {
            // Act
            var result = TextRules.NormalizeSpaces(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

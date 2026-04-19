using MicroServiceUsers.Domain.Validations;

namespace MicroServiceUsers.Domain.UnitTest
{
    public class TextRulesTests
    {
        #region NormalizeSpaces Tests
        [Theory]
        [InlineData(null, "")]
        [InlineData("  sistemas   ucb  ", "sistemas ucb")]
        public void NormalizeSpaces_WithVariousInputs_ReturnsNormalizedString(string? input, string expected)
        {
            var result = TextRules.NormalizeSpaces(input);
            Assert.Equal(expected, result);
        }
        #endregion

        #region CanonicalPersonName Tests
        [Theory]
        [InlineData("   ", "")]
        [InlineData(" luCAs  ", "Lucas")]
        [InlineData("l", "L")]
        public void CanonicalPersonName_WithVariousInputs_ReturnsCanonicalName(string? input, string expected)
        {
            var result = TextRules.CanonicalPersonName(input);
            Assert.Equal(expected, result);
        }
        #endregion

        #region IsValidLettersOnly Tests
        [Theory]
        [InlineData("", false)]
        [InlineData("Bolivia", true)]
        public void IsValidLettersOnly_WithVariousInputs_ReturnsExpectedResult(string? input, bool expected)
        {
            var result = TextRules.IsValidLettersOnly(input);
            Assert.Equal(expected, result);
        }
        #endregion

        #region IsValidLettersAndSpaces Tests
        [Theory]
        [InlineData(null, false)]
        [InlineData("Sistemas Computacionales", true)]
        public void IsValidLettersAndSpaces_WithVariousInputs_ReturnsExpectedResult(string? input, bool expected)
        {
            var result = TextRules.IsValidLettersAndSpaces(input);
            Assert.Equal(expected, result);
        }
        #endregion

        #region IsValidEmail Tests
        [Theory]
        [InlineData("   ", false)]
        [InlineData("lucas@ucb.edu.bo", true)]
        public void IsValidEmail_WithVariousInputs_ReturnsExpectedResult(string? input, bool expected)
        {
            var result = TextRules.IsValidEmail(input);
            Assert.Equal(expected, result);
        }
        #endregion

        #region IsValidUsername Tests
        [Theory]
        [InlineData("", false)]
        [InlineData("lucas.alcoba_2026", true)]
        public void IsValidUsername_WithVariousInputs_ReturnsExpectedResult(string? input, bool expected)
        {
            var result = TextRules.IsValidUsername(input);
            Assert.Equal(expected, result);
        }
        #endregion
    }
}

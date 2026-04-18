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



        // Pruebas para Funciones Canonical
        [Theory]
        [InlineData(null, "")] // TC1
        [InlineData("mAMANI", "Mamani")] // TC2
        public void CanonicalPersonName_Tests(string input, string expected)
        {
            var result = TextRules.CanonicalPersonName(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", "")] // TC3
        [InlineData("esto Es UNA prueba", "Esto Es UNA prueba")] // TC4
        public void CanonicalSentence_Tests(string input, string expected)
        {
            var result = TextRules.CanonicalSentence(input);
            Assert.Equal(expected, result);
        }

        // Pruebas para Validaciones de Texto
        [Theory]
        [InlineData("  ", false)] // TC5
        [InlineData("Juan", true)] // TC6
        [InlineData("Juan123", false)] // TC7
        public void IsValidLettersOnly_Tests(string input, bool expected)
        {
            var result = TextRules.IsValidLettersOnly(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)] // TC8
        [InlineData("De La Cruz", true)] // TC9
        [InlineData("Juan_2", false)] // TC10
        public void IsValidLettersAndSpaces_Tests(string input, bool expected)
        {
            var result = TextRules.IsValidLettersAndSpaces(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, false)]            // TC1: Nulo
        [InlineData("   ", false)]           // TC1: Espacios
        [InlineData("usuario.com", false)]    // TC2: Formato inválido
        [InlineData("test@mail.com", true)]   // TC3: Formato válido
        public void IsValidEmail_PathCoverage_Tests(string input, bool expected)
        {
            // Act
            var result = TextRules.IsValidEmail(input);

            // Assert
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData("", false)]           // TC1: Vacío
        [InlineData("123-ABC", false)]    // TC2: Formato incorrecto
        [InlineData("1234567-CB", true)]  // TC3: Formato válido
        public void IsValidBoliviaCi_PathCoverage_Tests(string input, bool expected)
        {
            // Act
            var result = TextRules.IsValidBoliviaCi(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "")] // TC1: Manejo de nulos
        [InlineData(" 1234567-lp  ", "1234567-LP")] // TC2: Normalización y Mayúsculas
        public void NormalizeCi_CodeCoverage_Tests(string input, string expected)
        {
            // Act
            var result = TextRules.NormalizeCi(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

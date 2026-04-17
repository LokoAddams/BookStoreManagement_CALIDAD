using MicroServiceDistributors.Domain.Models;
using MicroServiceDistributors.Domain.Validations;
using Xunit;

namespace MicroServiceDistributors.Domain.UnitTest
{
    public class DistributorValidationTests
    {
        
        [Theory]
        [InlineData("Dist X", "a@b.com", "12345678", "Av 1", 0, "TC1")]
        [InlineData("", "a@b.com", "12345678", "Av 1", 1, "TC2")]
        [InlineData("DistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDistDist", "a@b.com", "12345678", "Av 1", 1, "TC3")]
        [InlineData("Dist @#!", "a@b.com", "12345678", "Av 1", 1, "TC4")]
        [InlineData("Dist X", null, "12345678", "Av 1", 1, "TC5")]
        [InlineData("Dist X", "   ", "12345678", "Av 1", 1, "TC6")]
        [InlineData("Dist X", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@t", "12345678", "Av 1", 1, "TC7")]
        [InlineData("Dist X", "ab.com", "12345678", "Av 1", 1, "TC8")]
        [InlineData("Dist X", "a@b.com", "", "Av 1", 1, "TC9")]
        [InlineData("Dist X", "a@b.com", "123", "Av 1", 1, "TC10")]
        [InlineData("Dist X", "a@b.com", "12345678", "", 1, "TC11")]
        [InlineData("Dist X", "a@b.com", "12345678", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 1, "TC12")]
        public void Validate_ShouldReturnExpectedErrors(string name, string contactEmail, string phone, string address, int expectedErrorCount, string testCase)
        {
            var distributor = new Distributor
            {
                Name = name,
                ContactEmail = contactEmail,
                Phone = phone,
                Address = address
            };

            var errors = DistributorValidation.Validate(distributor).ToList();

            Assert.Equal(expectedErrorCount, errors.Count);
        }

        
        [Theory]
        [InlineData(" distribuidora SUR ", "VENTAS@Sur.com  ", " 123 456 78 ", " av. principal 123 ", false, "TC1")]
        [InlineData(" distribuidora SUR ", null, " 123 456 78 ", " av. principal 123 ", true, "TC2")]
        public void Normalize_ShouldNormalizeAllFields(string name, string contactEmail, string phone, string address, bool isEmailNull, string testCase)
        {
            var distributor = new Distributor
            {
                Name = name,
                ContactEmail = contactEmail,
                Phone = phone,
                Address = address
            };

            DistributorValidation.Normalize(distributor);

            // Nombre debe estar normalizado (capitalizado, sin espacios extras)
            Assert.Equal("Distribuidora SUR", distributor.Name);

            // Email debe estar normalizado y convertido a minúsculas
            if (isEmailNull)
            {
                // TC2: Email null debe convertirse en string.Empty
                Assert.Equal(string.Empty, distributor.ContactEmail);
            }
            else
            {
                // TC1: Email válido debe estar en minúsculas y sin espacios extras
                Assert.Equal("ventas@sur.com", distributor.ContactEmail);
            }

            // Teléfono debe estar normalizado (espacios extras removidos)
            Assert.Equal("123 456 78", distributor.Phone);

            // Dirección debe estar normalizada (capitalizada, sin espacios extras)
            Assert.Equal("Av. principal 123", distributor.Address);
        }
    }
}
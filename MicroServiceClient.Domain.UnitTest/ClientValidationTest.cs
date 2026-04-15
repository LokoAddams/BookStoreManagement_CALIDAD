using Xunit;
using System.Linq;
using System.Collections.Generic;
using MicroServiceClient.Domain.Validations;

namespace MicroServiceClient.Domain.UnitTest
{
    public class ClientValidationTest
    {
        [Theory]
        [InlineData("", "El CI es obligatorio.")] // TC1
        [InlineData("123456789012345678901", "El CI no debe superar")] // TC2
        [InlineData("ABC-12345", "El CI debe contener solo números")] // TC3
        [InlineData("1234567-LP", null)] // TC4 (Éxito)
        public void ValidateCi_Tests(string ciRaw, string expectedError)
        {
            // Act    
            var results = ClientValidation.ValidateCi(ciRaw).ToList();

            // Assert
            if (expectedError == null)
                Assert.Empty(results);
            else
                Assert.Contains(results, v => v.Message.Contains(expectedError));
        }

        [Theory]
        [InlineData("", "El nombre es obligatorio.")] // TC1
        [InlineData("Juan Alberto", "El nombre no debe contener espacios.")] // TC2
        [InlineData("EsteNombreEsDemasiadoLargoYSuperaLosCincuentaCaracteresPermitidos", "El nombre no debe superar")] // TC3
        [InlineData("Juan123", "El nombre solo puede contener letras.")] // TC4
        [InlineData("Juan", null)] // TC5 (Éxito)
        public void ValidateFirstName_Tests(string nameRaw, string expectedError)
        {
            var results = ClientValidation.ValidateFirstName(nameRaw).ToList();

            if (expectedError == null)
                Assert.Empty(results);
            else
                Assert.Contains(results, v => v.Message.Contains(expectedError));
        }

        [Theory]
        [InlineData(null, "El apellido es obligatorio.")] // TC1
        [InlineData("ApellidoLargo...", "El apellido no debe superar")] // TC2 (Usar string largo real en el proyecto)
        [InlineData("Mamani_4", "El apellido solo puede contener letras")] // TC3
        [InlineData("Mamani", null)] // TC4 (Éxito)
        public void ValidateLastName_Tests(string lastRaw, string expectedError)
        {
            // Para el TC2, simulamos el largo máximo configurado
            if (lastRaw == "ApellidoLargo...") lastRaw = new string('A', 101);

            var results = ClientValidation.ValidateLastName(lastRaw).ToList();

            if (expectedError == null)
                Assert.Empty(results);
            else
                Assert.Contains(results, v => v.Message.Contains(expectedError));
        }

        [Theory]
        [InlineData(null, "El correo electrónico es obligatorio.")] // TC1
        [InlineData("este.correo.es.extremadamente.largo.y.supera.los.ciento.cincuenta.caracteres.para.probar.la.validacion.de.longitud.maxima.del.campo.en.el.sistema.de.registro@example.com", "El correo no debe superar")] //TC2
        [InlineData("usuario.com", "Debe ingresar un correo electrónico válido.")] // TC3
        [InlineData("contacto@empresa.bo", null)] // TC4 (Éxito)
        public void ValidateEmail_Tests(string emailRaw, string expectedError)
        {
            var results = ClientValidation.ValidateEmail(emailRaw).ToList();

            if (expectedError == null)
                Assert.Empty(results);
            else
                Assert.Contains(results, v => v.Message.Contains(expectedError));
        }

        [Theory]
        [InlineData(null, "El número de teléfono es obligatorio.")] // TC1
        [InlineData("4421234", "exactamente 8 dígitos")] // TC2
        [InlineData("70712345", null)] // TC3 (Éxito)
        public void ValidatePhone_Tests(string phoneRaw, string expectedError)
        {
            var results = ClientValidation.ValidatePhone(phoneRaw).ToList();

            if (expectedError == null)
                Assert.Empty(results);
            else
                Assert.Contains(results, v => v.Message.Contains(expectedError));
        }

        [Theory]
        [InlineData(null, "La dirección es obligatoria.")] // TC1
        [InlineData("DireccionMuyLarga...", "La dirección no debe superar")] // TC2
        public void ValidateAddress_Tests(string addressRaw, string expectedError)
        {
            if (addressRaw == "DireccionMuyLarga...") addressRaw = new string('X', 500);

            var results = ClientValidation.ValidateAddress(addressRaw).ToList();

            Assert.Contains(results, v => v.Message.Contains(expectedError));
        }
    }
}
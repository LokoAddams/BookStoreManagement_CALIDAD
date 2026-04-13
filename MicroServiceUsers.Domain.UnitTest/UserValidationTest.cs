using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Domain.Validations;

namespace MicroServiceUsers.Domain.UnitTest
{
    public class UserValidationTest
    {
        [Fact]
        public void Normalize_TC1_Base_NormalizaTodosLosCampos()
        {
            var user = new User
            {
                Username = " User ",
                Email = " A@B.COM ",
                FirstName = " ana ",
                LastName = " perez ",
                MiddleName = " maria "
            };

            UserValidation.Normalize(user);

            Assert.Equal("user", user.Username);
            Assert.Equal("a@b.com", user.Email);
            Assert.Equal("Ana", user.FirstName);
            Assert.Equal("Perez", user.LastName);
            Assert.Equal("Maria", user.MiddleName);
        }

        [Fact]
        public void Normalize_TC2_UsernameNull_QuedaEmpty_YRestoNormalizado()
        {
            var user = new User
            {
                Username = null!,
                Email = " A@B.COM ",
                FirstName = " ana ",
                LastName = " perez ",
                MiddleName = " maria "
            };

            UserValidation.Normalize(user);

            Assert.Equal(string.Empty, user.Username);
            Assert.Equal("a@b.com", user.Email);
            Assert.Equal("Ana", user.FirstName);
            Assert.Equal("Perez", user.LastName);
            Assert.Equal("Maria", user.MiddleName);
        }

        [Fact]
        public void Normalize_TC3_EmailNull_QuedaEmpty_YRestoNormalizado()
        {
            var user = new User
            {
                Username = " User ",
                Email = null!,
                FirstName = " ana ",
                LastName = " perez ",
                MiddleName = " maria "
            };

            UserValidation.Normalize(user);

            Assert.Equal("user", user.Username);
            Assert.Equal(string.Empty, user.Email);
            Assert.Equal("Ana", user.FirstName);
            Assert.Equal("Perez", user.LastName);
            Assert.Equal("Maria", user.MiddleName);
        }

        [Fact]
        public void Normalize_TC4_FirstNameNull_SeIgnora_YRestoNormalizado()
        {
            var user = new User
            {
                Username = " User ",
                Email = " A@B.COM ",
                FirstName = null,
                LastName = " perez ",
                MiddleName = " maria "
            };

            UserValidation.Normalize(user);

            Assert.Equal("user", user.Username);
            Assert.Equal("a@b.com", user.Email);
            Assert.Null(user.FirstName);
            Assert.Equal("Perez", user.LastName);
            Assert.Equal("Maria", user.MiddleName);
        }

        [Fact]
        public void Normalize_TC5_LastNameNull_SeIgnora_YRestoNormalizado()
        {
            var user = new User
            {
                Username = " User ",
                Email = " A@B.COM ",
                FirstName = " ana ",
                LastName = null,
                MiddleName = " maria "
            };

            UserValidation.Normalize(user);

            Assert.Equal("user", user.Username);
            Assert.Equal("a@b.com", user.Email);
            Assert.Equal("Ana", user.FirstName);
            Assert.Null(user.LastName);
            Assert.Equal("Maria", user.MiddleName);
        }

        [Fact]
        public void Normalize_TC6_MiddleNameNull_SeIgnora_YRestoNormalizado()
        {
            var user = new User
            {
                Username = " User ",
                Email = " A@B.COM ",
                FirstName = " ana ",
                LastName = " perez ",
                MiddleName = null
            };

            UserValidation.Normalize(user);

            Assert.Equal("user", user.Username);
            Assert.Equal("a@b.com", user.Email);
            Assert.Equal("Ana", user.FirstName);
            Assert.Equal("Perez", user.LastName);
            Assert.Null(user.MiddleName);
        }

        [Fact]
        public void Validate_TC1_TodosLosDatosValidos_ListaVacia()
        {
            var user = CreateValidUser();

            var errors = UserValidation.Validate(user).ToList();

            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_TC2_UsernameVacio_ElUsernameEsObligatorio()
        {
            var user = CreateValidUser();
            user.Username = "";

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("El Username es obligatorio.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC3_UsernameMayorA50_NoDebeSuperar50Caracteres()
        {
            var user = CreateValidUser();
            user.Username = new string('u', 55);

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("No debe superar 50 caracteres.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC4_UsernameInvalido_MensajeFormatoUsername()
        {
            var user = CreateValidUser();
            user.Username = "ana!perez";

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("El nombre de usuario solo puede contener letras, números, puntos y guiones bajos.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC5_EmailNull_ElEmailEsObligatorio()
        {
            var user = CreateValidUser();
            user.Email = null!;

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("El Email es obligatorio.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC6_EmailMayorA150_NoDebeSuperar150Caracteres()
        {
            var user = CreateValidUser();
            user.Email = $"{new string('a', 151)}@mail.com";

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("No debe superar 150 caracteres.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC7_EmailInvalido_DebeIngresarCorreoValido()
        {
            var user = CreateValidUser();
            user.Email = "https://www.google.com/search?q=anamail.com";

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("Debe ingresar un correo electrónico válido.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC8_FirstNameMayorA50_ElNombreNoDebeSuperar()
        {
            var user = CreateValidUser();
            user.FirstName = new string('a', 55);

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("El nombre no debe superar 50 caracteres.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC9_FirstNameConEspacios_ElNombreNoDebeContenerEspacios()
        {
            var user = CreateValidUser();
            user.FirstName = "Ana Maria";

            var errors = UserValidation.Validate(user).ToList();

            Assert.Equal("El nombre no debe contener espacios.", errors[0].Message);
            Assert.Contains(errors, e => e.Message == "El nombre no debe contener espacios.");
        }

        [Fact]
        public void Validate_TC10_LastNameInvalido_ElApellidoSoloPuedeContenerLetrasYEspacios()
        {
            var user = CreateValidUser();
            user.LastName = "Perez 123";

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("El apellido solo puede contener letras y espacios.", errors[0].Message);
        }

        [Fact]
        public void Validate_TC11_MiddleNameMayorA50_ElSegundoNombreNoDebeSuperar()
        {
            var user = CreateValidUser();
            user.MiddleName = new string('m', 55);

            var errors = UserValidation.Validate(user).ToList();

            Assert.Single(errors);
            Assert.Equal("El segundo nombre no debe superar 50 caracteres.", errors[0].Message);
        }

        [Fact]
        public void ValidateField_TC1_IsRequiredTrue_ValueAbc_SinErrores()
        {
            var errors = InvokeValidateField("Username", "abc", isRequired: true, maxLength: 5);

            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateField_TC2_IsRequiredTrue_ValueVacio_ErrorObligatorio()
        {
            var errors = InvokeValidateField("Username", "", isRequired: true, maxLength: 5);

            Assert.Single(errors);
            Assert.Equal("El Username es obligatorio.", errors[0].Message);
        }

        [Fact]
        public void ValidateField_TC3_IsRequiredFalse_ValueAbc_SinErrores()
        {
            var errors = InvokeValidateField("Username", "abc", isRequired: false, maxLength: 5);

            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateField_TC4_IsRequiredTrue_ValueMayorMax_ErrorLongitud()
        {
            var errors = InvokeValidateField("Username", "abcdef", isRequired: true, maxLength: 5);

            Assert.Single(errors);
            Assert.Equal("No debe superar 5 caracteres.", errors[0].Message);
        }

        [Fact]
        public void ValidateField_TC5_IsRequiredFalse_ValueNull_SinErrores()
        {
            var errors = InvokeValidateField("Username", null, isRequired: false, maxLength: 5);

            Assert.Empty(errors);
        }

        private static List<ValidationError> InvokeValidateField(string fieldName, string? value, bool isRequired, int maxLength)
        {
            var method = typeof(UserValidation).GetMethod("ValidateField", BindingFlags.NonPublic | BindingFlags.Static)!;
            var result = (IEnumerable<ValidationError>)method.Invoke(null, new object?[] { fieldName, value, isRequired, maxLength })!;
            return result.ToList();
        }

        private static User CreateValidUser() => new()
        {
            Username = "ana.perez",
            Email = "ana.perez@mail.com",
            FirstName = "Ana",
            LastName = "Perez",
            MiddleName = "Maria"
        };
    }
}

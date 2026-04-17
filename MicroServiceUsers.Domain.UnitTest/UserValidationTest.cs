using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Domain.Validations;
using Xunit;

namespace MicroServiceUsers.Domain.UnitTest
{
    public class UserValidationTest
    {
        [Theory]
        [InlineData(" User ", " A@B.COM ", " ana ", " perez ", " maria ", "user", "a@b.com", "Ana", "Perez", "Maria")] // TC1
        [InlineData(null, " A@B.COM ", " ana ", " perez ", " maria ", "", "a@b.com", "Ana", "Perez", "Maria")] // TC2
        [InlineData(" User ", null, " ana ", " perez ", " maria ", "user", "", "Ana", "Perez", "Maria")] // TC3
        [InlineData(" User ", " A@B.COM ", null, " perez ", " maria ", "user", "a@b.com", null, "Perez", "Maria")] // TC4
        [InlineData(" User ", " A@B.COM ", " ana ", null, " maria ", "user", "a@b.com", "Ana", null, "Maria")] // TC5
        [InlineData(" User ", " A@B.COM ", " ana ", " perez ", null, "user", "a@b.com", "Ana", "Perez", null)] // TC6
        public void Normalize_Tests(string? username, string? email, string? firstName, string? lastName, string? middleName,
            string expectedUsername, string expectedEmail, string? expectedFirstName, string? expectedLastName, string? expectedMiddleName)
        {
            var user = new User
            {
                Username = username!,
                Email = email!,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName
            };

            UserValidation.Normalize(user);

            Assert.Equal(expectedUsername, user.Username);
            Assert.Equal(expectedEmail, user.Email);
            Assert.Equal(expectedFirstName, user.FirstName);
            Assert.Equal(expectedLastName, user.LastName);
            Assert.Equal(expectedMiddleName, user.MiddleName);
        }

        [Theory]
        [InlineData("ana.perez", "ana.perez@mail.com", "Ana", "Perez", "Maria", null)] // TC1: Todo válido
        [InlineData("", "ana.perez@mail.com", "Ana", "Perez", "Maria", "El Username es obligatorio.")] // TC2: Username vacío
        [InlineData("uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu", "ana.perez@mail.com", "Ana", "Perez", "Maria", "No debe superar 50 caracteres.")] // TC3: Username > 50
        [InlineData("ana!perez", "ana.perez@mail.com", "Ana", "Perez", "Maria", "El nombre de usuario solo puede contener letras, números, puntos y guiones bajos.")] // TC4: Username inválido
        [InlineData("ana.perez", null, "Ana", "Perez", "Maria", "El Email es obligatorio.")] // TC5: Email null
        [InlineData("ana.perez", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@mail.com", "Ana", "Perez", "Maria", "No debe superar 150 caracteres.")] // TC6: Email > 150
        [InlineData("ana.perez", "https://www.google.com/search?q=anamail.com", "Ana", "Perez", "Maria", "Debe ingresar un correo electrónico válido.")] // TC7: Email inválido
        [InlineData("ana.perez", "ana.perez@mail.com", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "Perez", "Maria", "El nombre no debe superar 50 caracteres.")] // TC8: FirstName > 50
        [InlineData("ana.perez", "ana.perez@mail.com", "Ana Maria", "Perez", "Maria", "El nombre no debe contener espacios.")] // TC9: FirstName con espacios
        [InlineData("ana.perez", "ana.perez@mail.com", "Ana", "Perez 123", "Maria", "El apellido solo puede contener letras y espacios.")] // TC10: LastName inválido
        [InlineData("ana.perez", "ana.perez@mail.com", "Ana", "Perez", "mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm", "El segundo nombre no debe superar 50 caracteres.")] // TC11: MiddleName > 50
        public void Validate_Tests(string username, string? email, string firstName, string lastName, string middleName, string? expectedError)
        {
            var user = new User
            {
                Username = username,
                Email = email!,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName
            };

            var errors = UserValidation.Validate(user).ToList();

            if (expectedError == null)
                Assert.Empty(errors);
            else
                Assert.Contains(errors, e => e.Message == expectedError);
        }

        [Theory]
        [InlineData("Username", "abc", true, 5, null)] // TC1: Válido
        [InlineData("Username", "", true, 5, "El Username es obligatorio.")] // TC2: Vacío cuando es obligatorio
        [InlineData("Username", "abc", false, 5, null)] // TC3: No obligatorio y válido
        [InlineData("Username", "abcdef", true, 5, "No debe superar 5 caracteres.")] // TC4: Supera max length
        [InlineData("Username", null, false, 5, null)] // TC5: Null cuando no es obligatorio
        public void ValidateField_Tests(string fieldName, string? value, bool isRequired, int maxLength, string? expectedError)
        {
            var errors = InvokeValidateField(fieldName, value, isRequired, maxLength);

            if (expectedError == null)
                Assert.Empty(errors);
            else
            {
                Assert.Single(errors);
                Assert.Equal(expectedError, errors[0].Message);
            }
        }

        [Theory]
        [InlineData("FirstName", "Ana", 5, "El nombre", false, null)] // TC1: Válido sin espacios
        [InlineData("FirstName", "", 5, "El nombre", false, null)] // TC2: Vacío (yield break)
        [InlineData("FirstName", "AnaMaria", 5, "El nombre", false, "El nombre no debe superar 5 caracteres.")] // TC3: Supera max length
        [InlineData("FirstName", "An M", 5, "El nombre", false, "multiple")] // TC4: Espacios y caracteres inválidos (dos errores)
        [InlineData("LastName", "An M", 5, "El apellido", true, null)] // TC5: Válido con espacios permitidos
        [InlineData("FirstName", "Ana1", 5, "El nombre", false, "El nombre solo puede contener letras.")] // TC6: Formato inválido
        [InlineData("LastName", "An 1", 5, "El apellido", true, "El apellido solo puede contener letras y espacios.")] // TC7: Formato con espacios permitidos
        public void ValidateNameField_Tests(string fieldName, string? value, int maxLength, string label, bool allowSpaces, string? expectedError)
        {
            var errors = InvokeValidateNameField(fieldName, value, maxLength, label, allowSpaces);

            if (expectedError == null)
                Assert.Empty(errors);
            else if (expectedError == "multiple")
            {
                Assert.Equal(2, errors.Count);
                Assert.Contains(errors, e => e.Message == "El nombre no debe contener espacios.");
                Assert.Contains(errors, e => e.Message == "El nombre solo puede contener letras.");
            }
            else
            {
                Assert.Single(errors);
                Assert.Equal(expectedError, errors[0].Message);
            }
        }

        private static List<ValidationError> InvokeValidateField(string fieldName, string? value, bool isRequired, int maxLength)
        {
            var method = typeof(UserValidation).GetMethod("ValidateField", BindingFlags.NonPublic | BindingFlags.Static)!;
            var result = (IEnumerable<ValidationError>)method.Invoke(null, new object?[] { fieldName, value, isRequired, maxLength })!;
            return result.ToList();
        }

        private static List<ValidationError> InvokeValidateNameField(string fieldName, string? value, int maxLength, string label, bool allowSpaces = false)
        {
            var method = typeof(UserValidation).GetMethod("ValidateNameField", BindingFlags.NonPublic | BindingFlags.Static)!;
            var result = (IEnumerable<ValidationError>)method.Invoke(null, new object?[] { fieldName, value, maxLength, label, allowSpaces })!;
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

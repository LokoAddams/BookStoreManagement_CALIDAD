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
    }
}

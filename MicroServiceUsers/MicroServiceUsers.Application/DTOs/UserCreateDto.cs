using System.ComponentModel.DataAnnotations;

namespace MicroServiceUsers.Application.DTOs
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Role { get; set; } = "User";
    }
}

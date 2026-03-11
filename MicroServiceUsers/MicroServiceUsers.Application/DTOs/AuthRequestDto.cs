using System.ComponentModel.DataAnnotations;

namespace MicroServiceUsers.Application.DTOs
{
    public class AuthRequestDto
    {
        [Required(ErrorMessage = "El campo Usuario o Correo es obligatorio.")]
        [Display(Name = "Usuario o Correo")]
        public string UserOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }
}

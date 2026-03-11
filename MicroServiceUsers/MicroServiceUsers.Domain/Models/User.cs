using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceUsers.Domain.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Nombre de usuario")]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no debe superar los 50 caracteres.")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
        [StringLength(150, ErrorMessage = "El correo no debe superar los 150 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El nombre no debe superar los 50 caracteres.")]
        public string? FirstName { get; set; }

        [Display(Name = "Apellidos")]
        [StringLength(100, ErrorMessage = "El apellido no debe superar los 100 caracteres.")]
        public string? LastName { get; set; }

        [Display(Name = "Segundo nombre")]
        [StringLength(50, ErrorMessage = "El segundo nombre no debe superar los 50 caracteres.")]
        public string? MiddleName { get; set; }

        [Display(Name = "Hash de contraseña")]
        [Required(ErrorMessage = "El hash de contraseña es obligatorio.")]
        public string PasswordHash { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Debe cambiar contraseña")]
        public bool MustChangePassword { get; set; } = true;

        [Display(Name = "Fecha de creación")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace MicroServiceUsers.Domain.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Nombre del rol")]
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del rol no debe superar los 50 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(200, ErrorMessage = "La descripción no debe superar los 200 caracteres.")]
        public string? Description { get; set; }
    }
}

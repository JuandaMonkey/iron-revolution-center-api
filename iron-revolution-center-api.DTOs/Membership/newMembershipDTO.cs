using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Membership
{
    public class newMembershipDTO
    {
        [Required(ErrorMessage = "Se requiere el nombre.")] // required
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")] // length
        public string? Nombre { get; set; } // name 

        [Required(ErrorMessage = "Se requiere la duración.")] // required
        [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser un número positivo.")]
        public int? Duracion { get; set; } // duration membership in days
    }
}

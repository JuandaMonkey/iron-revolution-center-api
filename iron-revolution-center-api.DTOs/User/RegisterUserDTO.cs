using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.User
{
    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "Se requiere un nombre de usuario.")] // requireds
        [StringLength(12, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 12 caracteres.")] // length
        public string? Nombre_Usuario { get; set; } // user name

        [Required(ErrorMessage = "Se requiere la contraseña.")] // required
        [StringLength(12, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 12 caracteres.")] // length
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)[A-Za-z0-9áéíóúÁÉÍÓÚñÑ]{8,12}$", ErrorMessage = "La contraseña debe tener al menos una mayúscula y un número.")]
        public string? Contraseña { get; set; } // password

        public string? Rol { get; set; } // role

        public string? NIP { get; set; } // identification
    }
}

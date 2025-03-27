using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.User
{
    public class ModifyPassworDTO
    {
        [Required(ErrorMessage = "Se requiere la contraseña.")] // required
        [StringLength(12, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 12 caracteres.")] // length
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,12}$", ErrorMessage = "La contraseña debe tener al menos una mayúscula y un número.")]
        public string? Contraseña { get; set; } // password
    }
}

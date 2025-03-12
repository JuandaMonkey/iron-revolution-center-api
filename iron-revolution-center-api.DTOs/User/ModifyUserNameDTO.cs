using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.User
{
    public class ModifyUserNameDTO
    {
        [Required(ErrorMessage = "Se requiere un nombre de usuario.")] // requireds
        [StringLength(12, MinimumLength = 3, ErrorMessage = "La contraseña debe tener entre 3 y 12 caracteres.")] // length
        public string? Nombre_Usuario { get; set; } // user name
    }
}

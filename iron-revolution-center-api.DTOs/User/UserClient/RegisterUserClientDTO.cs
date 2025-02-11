using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.User.UserClient
{
    // db.Clients.InsertOne
    public class RegisterUserClientDTO
    {
        [BsonElement("User_Name")]
        [Required(ErrorMessage = "Se requiere un nombre de usuario.")] // requireds
        public string? User_Name { get; set; } // user name

        [BsonElement("Password")]
        [Required(ErrorMessage = "Se requiere la contraseña.")] // required
        [StringLength(12, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 12 caracteres.")] // length
        public string? Password { get; set; } // password

        [BsonElement("Role")]
        public string? Role { get; set; } // role
    }
}

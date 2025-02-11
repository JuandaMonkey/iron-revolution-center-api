using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace iron_revolution_center_api.DTOs.Client
{
    // db.Clients.InsertOne
    public class RegisterClientDTO
    {
        [BsonElement("NIP")]
        public string? NIP { get; set; } // identifier

        [BsonElement("Photo")]
        public string? Photo { get; set; } = "";  // photo

        [BsonElement("Full_Name")]
        [Required(ErrorMessage = "Se requiere el nombre completo.")] // required
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 500 caracteres.")] // length
        public string? Full_Name { get; set; }  // full name

        [BsonElement("Phone")]
        [Required(ErrorMessage = "Se requiere el número celular.")] // required
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de celular debe tener exactamente 10 dígitos.")] // cell phone number validation
        public string? Phone { get; set; }  // phone

        [BsonElement("Observation")]
        [StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")] // length
        public string? Observation { get; set; } = ""; // observation
    }
}

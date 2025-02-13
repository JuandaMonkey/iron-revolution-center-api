using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Staff
{
    public class RegisterStaffDTO
    {
        [BsonElement("NIP")]
        public string? NIP { get; set; } // identifier

        [BsonElement("Photo")]
        public string? Photo { get; set; }  // photo

        [BsonElement("Full_Name")]
        [Required(ErrorMessage = "Se requiere el nombre completo.")] // required
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 500 caracteres.")] // length
        public string? Full_Name { get; set; }  // full name

        [BsonElement("Phone")]
        [Required(ErrorMessage = "Se requiere el número celular.")] // required
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de celular debe tener exactamente 10 dígitos.")] // cell phone number validation
        public string? Phone { get; set; }  // phone

        [BsonElement("Branch_Office")]
        [Required(ErrorMessage = "Se requiere la sucursal.")] // required
        public string? Branch_Office { get; set; }  // branch office
    }
}

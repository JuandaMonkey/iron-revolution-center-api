using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Branch_Office
{
    public class InsertBranche_OfficeDTO
    {
        [BsonElement("Branche_ID")]
        public string? Branche_ID { get; set; }  // identification

        [BsonElement("Name")]
        [Required(ErrorMessage = "Se requiere el nombre de la sucursal.")] // required
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El nombre de la sucursal debe tener entre 3 y 200 caracteres.")] // length
        public string? Name { get; set; }  // name

        [BsonElement("Location")]
        [Required(ErrorMessage = "Se requiere la ubicación de la sucursal.")] // required
        public string? Location { get; set; }  // location
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Membership
{
    // db.Memberships
    public class InsertMembershipDTO
    {
        [BsonElement("Membership_ID")]
        public string? Membership_ID { get; set; } // identification

        [BsonElement("Name")]
        [Required(ErrorMessage = "Se requiere el nombre.")] // required
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")] // length
        public string? Name { get; set; } // name membership

        [BsonElement("Duration")]
        [Required(ErrorMessage = "Se requiere la duración.")] // required
        [RegularExpression(@"^\d+$", ErrorMessage = "La duración debe contener solo números popsitivos.")]
        public int? Duration { get; set; } // duration membership in days
    }
}

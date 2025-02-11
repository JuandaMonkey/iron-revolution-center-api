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
    public class ModifyMembershipDTO
    {
        [BsonElement("Name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")] // length
        public string? Name { get; set; } // name membership

        [BsonElement("Duration")]
        public int? Duration { get; set; } // duration membership in days
    }
}

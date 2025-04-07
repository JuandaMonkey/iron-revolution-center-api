using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Membership
{
    public class InsertMembershipDTO
    {
        public string? Membresia_Id { get; set; } 

        public string? Nombre { get; set; } 

        public int? Duracion { get; set; } 
    }
}

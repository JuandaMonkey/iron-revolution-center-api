using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    // db.Memberships
    public class MembershipsModel
    {
        public string? Membresia_ID { get; set; } // identification

        public string? Nombre { get; set; } // name membership

        public int Duracion { get; set; } // duration membership
    }
}

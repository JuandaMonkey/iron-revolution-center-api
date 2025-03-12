using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class RolesModel
    {
        public string? Rol_Id { get; set; } // identifier

        public string? Nombre { get; set; } // name
    }
}

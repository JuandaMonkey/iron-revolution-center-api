using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class BranchesModel
    {
        public string? Sucursal_Id { get; set; } // identification

        public string? Nombre { get; set; } // name

        public string? Ubicacion { get; set; } // location
    }
}

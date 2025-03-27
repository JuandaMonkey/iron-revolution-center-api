using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class UsersModel
    {
        public string? Nombre_Usuario { get; set; } // user name

        public string? Contraseña { get; set; } // password

        public string? Rol { get; set; } // role

        public string? NIP { get; set; } // NIP
    }
}

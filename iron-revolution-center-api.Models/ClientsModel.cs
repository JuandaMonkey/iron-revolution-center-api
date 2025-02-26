using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace iron_revolution_center_api.Models
{
    // db.clients
    public class ClientsModel
    {
        public string? NIP { get; set; } // identifier

        public string? Foto { get; set; }  // photo

        public string? Nombre_Completo { get; set; }  // full name

        public string? Celular { get; set; }  // phone

        public string? Observacion { get; set; }  // observation

        public string? Membresia { get; set; } // membership

        public DateOnly? Fecha_Inicio { get; set; } // date_start

        public DateOnly? Fecha_Fin { get; set; } // date_end

        public bool Estado { get; set; } // status
    }
}

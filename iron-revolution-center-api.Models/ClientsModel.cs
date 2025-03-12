using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class ClientsModel
    {
        public string? NIP { get; set; } // identifier

        public byte[]? Foto { get; set; } // photo

        public string? Clave_Seguridad { get; set; } // segurity key

        public string? Nombre_Completo { get; set; } // full name

        public string? Celular { get; set; } // phone

        public string? Observacion { get; set; } // observation

        public string? Membresia { get; set; } // membership

        public DateOnly? Fecha_Inicio { get; set; } // date start

        public DateOnly? Fecha_Fin { get; set; } // date end
    }
}

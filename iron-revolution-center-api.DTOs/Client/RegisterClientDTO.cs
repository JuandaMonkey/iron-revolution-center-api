using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using iron_revolution_center_api.Models;

namespace iron_revolution_center_api.DTOs.Client
{
    public class RegisterClientDTO
    {
        public string? NIP { get; set; } // identifier

        public byte[]? Foto { get; set; }  // photo

        public string? Clave_Seguridad { get; set; } // segurity_key

        public string? Nombre_Completo { get; set; } // full name

        public string? Celular { get; set; } // phone

        public string? Observacion { get; set; } // observation

        public BranchesModel? Sucursal { get; set; } // branch
    }
}

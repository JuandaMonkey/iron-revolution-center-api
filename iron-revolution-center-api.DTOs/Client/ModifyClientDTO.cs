using iron_revolution_center_api.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Client
{
    public class ModifyClientDTO
    {
        public byte[]? Foto { get; set; } // photo

        public string? Nombre_Completo { get; set; }  // full name

        public string? Celular { get; set; }  // phone

        public string? Observacion { get; set; } // observation

        public BranchesModel? Sucursal { get; set; } // branch
    }
}

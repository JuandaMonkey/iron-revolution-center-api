using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Branch_Office
{
    public class ModifyBranche_OfficeDTO
    {
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El nombre de la sucursal debe tener entre 3 y 200 caracteres.")] // length
        public string? Nombre { get; set; }  // name

        public string? Ubicacion { get; set; }  // location
    }
}

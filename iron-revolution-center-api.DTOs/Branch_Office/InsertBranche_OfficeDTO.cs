using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Branch_Office
{
    public class InsertBranche_OfficeDTO 
    { 
        public string? Sucursal_ID { get; set; }  // identification

        [Required(ErrorMessage = "Se requiere el nombre de la sucursal.")] // required
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El nombre de la sucursal debe tener entre 3 y 200 caracteres.")] // length
        public string? Nombre { get; set; }  // name

        [Required(ErrorMessage = "Se requiere la ubicación de la sucursal.")] // required
        public string? Ubicacion { get; set; }  // location
    }
}

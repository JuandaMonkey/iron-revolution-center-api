using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Client
{
    public class modifyClientModel
    {
        public byte[]? Foto { get; set; } // photo

        [StringLength(500, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 500 caracteres.")] // length
        public string? Nombre_Completo { get; set; }  // full name

        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de celular debe tener exactamente 10 dígitos.")] // cell phone number validation
        public string? Celular { get; set; }  // phone

        [StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")] // length
        public string? Observacion { get; set; } // observation

        public string? sucursal_Id { get; set; } // branch id
    }
}

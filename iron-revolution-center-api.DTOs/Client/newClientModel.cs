using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Client
{
    public class newClientModel
    {
        public string? Foto { get; set; }  // photo

        [Required(ErrorMessage = "Se requiere el nombre completo.")] // required
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 500 caracteres.")] // length
        public string? Nombre_Completo { get; set; } // full name

        [Required(ErrorMessage = "Se requiere el número celular.")] // required
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de celular debe tener exactamente 10 dígitos.")] // cell phone number validation
        public string? Celular { get; set; } // phone

        [StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")] // length
        public string? Observacion { get; set; } // observation

        [Required(ErrorMessage = "Se requiere la sucursal.")] // required
        public string? sucursal_Id { get; set; } // branch id
    }
}

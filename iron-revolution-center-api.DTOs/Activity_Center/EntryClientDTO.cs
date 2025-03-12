using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Activity_Center
{
    public class EntryClientDTO
    {
        [Required(ErrorMessage = "Se requiere el NIP del cliente.")] // required
        public string Cliente { get; set; } = string.Empty; // identifier

        public DateTime Entrada { get; set; } // fecha y hora de entrada

        public DateTime Salida { get; set; } // fecha y salida de entrada

        [Required(ErrorMessage = "Se requiere el ID de la sucursal.")] // required
        public string Sucursal { get; set; } = string.Empty; // identifier
    }
}

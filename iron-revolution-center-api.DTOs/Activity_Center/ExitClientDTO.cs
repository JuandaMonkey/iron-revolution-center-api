using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Activity_Center
{
    public class ExitClientDTO
    {
        [Required(ErrorMessage = "Se requiere el NIP del cliente.")] // required
        public string? Cliente { get; set; } // identifier

        public DateTime? Entrada { get; set; } // fecha y hora de entrada

        public DateTime? Salida { get; set; } // fecha y salida de entrada
    }
}

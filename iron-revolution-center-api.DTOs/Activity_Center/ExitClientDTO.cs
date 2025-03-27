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
        [Required(ErrorMessage = "Se requiere el NIP del cliente.")] 
        public string? Cliente { get; set; } 

        public DateTime Entrada { get; set; }

        public DateTime Salida { get; set; } 
    }
}

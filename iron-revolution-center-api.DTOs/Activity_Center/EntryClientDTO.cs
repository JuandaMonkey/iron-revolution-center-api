using iron_revolution_center_api.Models;
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
        [Required(ErrorMessage = "Se requiere el NIP del cliente.")] 
        public ClientsModel? Cliente { get; set; } 

        public DateTime Entrada { get; set; } 

        public DateTime? Salida { get; set; } 

        [Required(ErrorMessage = "Se requiere el ID de la sucursal.")] 
        public BranchesModel? Sucursal { get; set; }
    }
}

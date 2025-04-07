using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Exercise
{
    public class newExerciseDTO
    {
        public byte[]? Foto { get; set; } 

        [Required(ErrorMessage = "Se requiere el nombre.")] 
        public string? Nombre { get; set; } 

        [Required(ErrorMessage = "Se requiere el tipo.")] 
        public string? Tipo { get; set; } 

        [Required(ErrorMessage = "Se requiere una descripción.")] 
        public string? Descripcion { get; set; } 

        [Required(ErrorMessage = "Se requiere las series.")] 
        [Range(1, int.MaxValue, ErrorMessage = "Las series deben ser un número positivo.")]
        public int Series { get; set; }  

        [Required(ErrorMessage = "Se requiere las repeticiones.")] 
        [Range(1, int.MaxValue, ErrorMessage = "Las repeticiones deben ser un número positivo.")]
        public int Repeticiones { get; set; } 
    }
}

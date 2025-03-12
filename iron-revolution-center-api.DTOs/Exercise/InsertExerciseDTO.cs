using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Exercise
{
    public class InsertExerciseDTO
    {
        public string? Ejercicio_Id { get; set; } // identifier

        public byte[]? Foto { get; set; } // photo

        [Required(ErrorMessage = "Se requiere el nombre.")] // required
        public string? Nombre { get; set; } // name

        [Required(ErrorMessage = "Se requiere el tipo.")] // required
        public string? Tipo { get; set; } // type

        [Required(ErrorMessage = "Se requiere una descripción.")] // required
        public string? Descripcion { get; set; } // description

        [Required(ErrorMessage = "Se requiere las series.")] // required
        [Range(1, int.MaxValue, ErrorMessage = "Las series deben ser un número positivo.")]
        public int Series { get; set; }  // series

        [Required(ErrorMessage = "Se requiere las repeticiones.")] // required
        [Range(1, int.MaxValue, ErrorMessage = "Las repeticiones deben ser un número positivo.")]
        public int Repeticiones { get; set; }  // repetitions
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Exercise
{
    public class ModifyExerciseDTO
    {
        public byte[]? Foto { get; set; } // photo

        public string? Nombre { get; set; } // name

        public string? Tipo { get; set; } // type

        public string? Descripcion { get; set; } // description

        [Range(1, int.MaxValue, ErrorMessage = "Las series deben ser un número positivo.")]
        public int? Series { get; set; }  // series

        [Range(1, int.MaxValue, ErrorMessage = "Las repeticiones deben ser un número positivo.")]
        public int? Repeticiones { get; set; }  // repetitions
    }
}

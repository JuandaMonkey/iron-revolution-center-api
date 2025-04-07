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
        public string? Ejercicio_Id { get; set; } 

        public byte[]? Foto { get; set; } 

        public string? Nombre { get; set; } 

        public string? Tipo { get; set; } 

        public string? Descripcion { get; set; }

        public int Series { get; set; }  

        public int Repeticiones { get; set; } 
    }
}

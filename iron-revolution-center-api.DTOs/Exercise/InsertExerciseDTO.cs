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
        [BsonElement("Exercise_ID")]
        public string? Exercise_ID { get; set; } // identifier

        [BsonElement("Photo")]
        public string? Photo { get; set; }  // photo

        [BsonElement("Name")]
        [Required(ErrorMessage = "Se requiere el nombre.")] // required
        public string? Name { get; set; }  // name

        [BsonElement("Description")]
        [Required(ErrorMessage = "Se requiere una descripción.")] // required
        public string? Description { get; set; }  // description

        [BsonElement("Series")]
        [Required(ErrorMessage = "Se requiere las series.")] // required
        [Range(1, int.MaxValue, ErrorMessage = "Las series deben ser un número positivo.")]
        public int Series { get; set; }  // series

        [BsonElement("Repetitions")]
        [Required(ErrorMessage = "Se requiere las repeticiones.")] // required
        [Range(1, int.MaxValue, ErrorMessage = "Las repeticiones deben ser un número positivo.")]
        public int Repetitions { get; set; }  // series
    }
}

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
        [BsonElement("Photo")]
        public string? Photo { get; set; }  // photo

        [BsonElement("Name")]
        public string? Name { get; set; }  // name

        [BsonElement("Description")]
        public string? Description { get; set; }  // description

        [BsonElement("Series")]
        [Range(1, int.MaxValue, ErrorMessage = "Las series deben ser un número positivo.")]
        public int? Series { get; set; }  // series

        [BsonElement("Repetitions")]
        [Range(1, int.MaxValue, ErrorMessage = "Las repeticiones deben ser un número positivo.")]
        public int? Repetitions { get; set; }  // series
    }
}

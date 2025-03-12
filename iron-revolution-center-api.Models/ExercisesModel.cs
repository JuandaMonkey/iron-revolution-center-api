using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    // db.Exercises
    public class ExercisesModel
    {
        public string? Ejercicio_Id { get; set; } // identifier

        public byte[]? Foto { get; set; } // photo

        public string? Nombre { get; set; } // name

        public string? Tipo { get; set; } // type

        public string? Descripcion { get; set; } // description

        public int Series { get; set; }  // series

        public int Repeticiones { get; set; }  // repetitions
    }
}

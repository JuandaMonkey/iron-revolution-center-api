using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class ExercisesModel
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id

        [BsonElement("Exercise_ID")]
        public string? Exercise_ID { get; set; } // identifier

        [BsonElement("Photo")]
        public string? Photo { get; set; }  // photo

        [BsonElement("Name")]
        public string? Name { get; set; }  // name

        [BsonElement("Description")]
        public string? Description { get; set; }  // description

        [BsonElement("Series")]
        public int Series { get; set; }  // series

        [BsonElement("Repetitions")]
        public int Repetitions { get; set; }  // series
    }
}

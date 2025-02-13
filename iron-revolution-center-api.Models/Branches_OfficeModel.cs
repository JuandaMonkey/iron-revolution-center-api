using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class Branches_OfficeModel
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id

        [BsonElement("Branche_ID")]
        public string? Branche_ID { get; set; }  // identification

        [BsonElement("Name")]
        public string? Name { get; set; }  // name

        [BsonElement("Location")]
        public string? Location { get; set; }  // location
    }
}

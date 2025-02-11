using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace iron_revolution_center_api.Models
{
    // db.clients
    public class ClientsModel
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id

        [BsonElement("NIP")]
        public string? NIP { get; set; } // identifier

        [BsonElement("Photo")]
        public string? Photo { get; set; }  // photo

        [BsonElement("Full_Name")]
        public string? Full_Name { get; set; }  // full name

        [BsonElement("Phone")]
        public string? Phone { get; set; }  // phone

        [BsonElement("Observation")]
        public string? Observation { get; set; }  // observation
    }
}

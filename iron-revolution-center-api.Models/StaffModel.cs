using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class StaffModel
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

        [BsonElement("Branch_Office")]
        public string? Branch_Office { get; set; }  // branch office
    }
}

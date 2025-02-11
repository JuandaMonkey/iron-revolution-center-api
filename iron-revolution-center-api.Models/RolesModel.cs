using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    // db.Roles
    public class RolesModel
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id

        [BsonElement("Role_ID")]
        public string? Role_ID { get; set; } // identifier

        [BsonElement("Name")]
        public string? Name { get; set; } // name
    }
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class UsersModel
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id

        [BsonElement("User_Name")]
        public string? User_Name { get; set; } // user name

        [BsonElement("Password")]
        public string? Password { get; set; } // password

        [BsonElement("Role")]
        public string? Role { get; set; } // role

        [BsonElement("NIP")]
        public string? NIP { get; set; } // nip
    }
}

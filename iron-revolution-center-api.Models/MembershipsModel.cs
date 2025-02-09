using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    // db.Memberships
    public class MembershipsModel
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id

        [BsonElement("Membership_ID")]
        public string? Membership_ID { get; set; } // identification

        [BsonElement("Name")]
        public string? Name { get; set; } // name membership

        [BsonElement("Duration")]
        public int? Duration { get; set; } // duration membership
    }
}

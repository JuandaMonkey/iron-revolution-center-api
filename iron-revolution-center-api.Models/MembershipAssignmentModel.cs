using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class MembershipAssignmentModel
    {
        [BsonId]
        public ObjectId _id { get; set; } 

        public string? NIP { get; set; }

        public string? Membership_ID { get; set; }

        public DateTime Start_Date { get; set; }

        public DateTime End_Date { get; set; }

        public bool Status { get; set; }
    }
}

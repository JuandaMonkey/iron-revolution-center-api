using iron_revolution_center_api.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.MembershipAssignment
{
    public class MembershipDetailDTO
    {
        [BsonId]
        public ObjectId _id { get; set; }

        public ClientsModel? Client { get; set; }

        public MembershipsModel? Membership { get; set; }

        public DateTime Start_Date { get; set; }

        public DateTime End_Date { get; set; }

        public bool Status { get; set; }
    }
}

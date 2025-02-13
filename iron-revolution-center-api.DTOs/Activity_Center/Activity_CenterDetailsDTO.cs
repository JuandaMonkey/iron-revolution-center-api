using iron_revolution_center_api.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Activity_Center
{
    public class Activity_CenterDetailsDTO
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id
        public ClientsModel? Client { get; set; }
        public DateTime date_entry { get; set; }
        public DateTime date_exit { get; set; }
        public Branches_OfficeModel? branch_office { get; set; }
    }
}

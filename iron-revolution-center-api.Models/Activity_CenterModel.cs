using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class Activity_CenterModel
    {
        [BsonId]
        public ObjectId _id { get; set; } // _id
        public string? NIP { get; set; }
        public DateTime date_entry { get; set; }
        public DateTime date_exit { get; set; }
        public string? branch_office { get; set; }
    }
}

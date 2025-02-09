using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    
namespace iron_revolution_center_api.DTOs.Users
{
    // db.Users.InsertOne
    public class RegisterUserDTOs
    {
        [BsonElement("User_Name")]
        [Required] // required
        [RegularExpression(@"^[a-z_]+$")] // can not use contain space
        public string? User_Name { get; set; } // user name

        [BsonElement("Role")]
        [RegularExpression(@"^(?=.*[A-Z])(?!.*\d)[A-Za-z\d]+$")] // must contain letters and numbers
        public string? Role { get; set; } = "Cliente"; // role

        [BsonElement("Password")]
        [Required] // required
        [RegularExpression(@"^(?=.*[A-Z])(?!.*\s).+$")] // must contain at least one capital letter and must not contain any space
        public string? Password { get; set; } // password
    }
}

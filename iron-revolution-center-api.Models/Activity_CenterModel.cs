using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class Activity_CenterModel
    {
        public ClientsModel? Cliente { get; set; }
    
        public DateTime? Entrada { get; set; }

        public DateTime? Salida { get; set; }

        public BranchesModel? Sucursal { get; set; }
    }
}

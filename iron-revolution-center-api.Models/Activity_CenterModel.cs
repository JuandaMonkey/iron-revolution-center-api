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
        public ClientsModel? Cliente { get; set; } // identifier
    
        public DateTime? Entrada { get; set; } // fecha y hora de entrada

        public DateTime? Salida { get; set; } // fecha y salida de entrada

        public BranchesModel? Sucursal { get; set; } // identifier
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class EmployeesModel
    {
        public string? NIP { get; set; } // identifier

        public byte[]? Foto { get; set; } // photo

        public string? Nombre_Completo { get; set; } // full name

        public string? Celular { get; set; } // phone

        public BranchesModel? Sucursal { get; set; }  // branch 
    }
}

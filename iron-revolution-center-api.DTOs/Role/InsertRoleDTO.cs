﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Role
{
    public class InsertRoleDTO
    {
        public string? Rol_Id { get; set; } // identifier

        [Required(ErrorMessage = "El nombre del rol es requerido.")]  // required
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre 3 y 100 caracteres.")] // length
        public string? Nombre { get; set; } // name
    }
}

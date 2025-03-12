using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.Membership
{
    public class AssignMembershipDTO
    {
        [Required(ErrorMessage = "Se requiere el NIP del cliente.")] // required
        public string? NIP { get; set; } // identification

        [Required(ErrorMessage = "Se requiere el ID de la membresía.")] // required
        public string? Membresia_Id { get; set; } // identification
    }
}

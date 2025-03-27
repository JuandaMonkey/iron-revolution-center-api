using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.DTOs.User
{
    public class ShowUserDTO
    {
        public UsersModel? Usuario { get; set; }

        public EmployeesModel? Empleado { get; set; }

        public ClientsModel? Cliente { get; set; }
    }
}

using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    public interface iEmployeesService
    {
        // list of employees
        public Task<IEnumerable<EmployeesModel>> ListEmployees();

        // get employee by nip
        public Task<EmployeesModel> GetEmployeeByNIP(string NIP);

        // register employee
        public Task<EmployeesModel> RegisterEmployee(RegisterEmployeeDTO employeeDTO);

        // modify employee
        public Task<EmployeesModel> ModifyEmployee(string NIP, ModifyEmployeeDTO employeeDTO);

        // delete employee
        public Task<EmployeesModel> DeleteEmployee(string NIP);
    }
}

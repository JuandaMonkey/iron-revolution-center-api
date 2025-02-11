using iron_revolution_center_api.DTOs.Role;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    // interface for the RolesService
    public interface iRolesService
    {
        // list of roles
        public Task<IEnumerable<RolesModel>> ListRoles();

        // get a role by identification
        public Task<IEnumerable<RolesModel>> GetRoleByID(string roleID);

        // insert role
        public Task<InsertRoleDTO> InsertRole(InsertRoleDTO roleDTO);

        // modify role
        public Task<RolesModel> ModifyRole(string roleID, ModifyRoleDTO roleDTO);

        // delete role
        public Task<RolesModel> DeleteRole(string roleID);
    }
}

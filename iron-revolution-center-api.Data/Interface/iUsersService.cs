using iron_revolution_center_api.DTOs.User;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    // interface for the UsersService
    public interface iUsersService
    {
        // list of users
        public Task<IEnumerable<UsersModel>> ListUsers();

        // list of users by role
        public Task<IEnumerable<UsersModel>> ListUsersByRole(string roleID);

        // get user by id
        public Task<IEnumerable<UsersModel>> GetUsersByUserName(string userName);

        // register user 
        public Task<RegisterUserDTO> RegisterUser(RegisterUserDTO userDTO);

        // register user client
        public Task<RegisterUserDTO> RegisterUserClient(RegisterUserDTO userDTO);

        // modify user name
        public Task<UsersModel> ModifyUserName(string userName, ModifyUserNameDTO userNameDTO);

        // modify password
        public Task<UsersModel> ModifyPassword(string userName, ModifyPassworDTO passwordDTO);

        // delete user
        public Task<UsersModel> DeleteUser(string userName);
    }
}

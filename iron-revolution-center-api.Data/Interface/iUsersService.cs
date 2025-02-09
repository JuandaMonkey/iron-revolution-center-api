using iron_revolution_center_api.DTOs.Users;
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
        // get an user by user name
        public Task<UsersModel> GetUserByUsername(string userName);

        // register an user
        public Task<RegisterUserDTOs> RegisterUser(RegisterUserDTOs userDto);

        // login an user
        public Task<UsersModel> LoginUser(string userName, string password);

        // assing client by nip to user
        public Task<UsersModel> AssignClient(string user_name, string password, string NIP);
    }
}

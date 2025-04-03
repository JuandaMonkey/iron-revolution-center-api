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

        // get user
        public Task<ShowUserDTO> GetUsersByUserName(string userName);

        // register user 
        public Task<ShowUserDTO> RegisterUser(RegisterUserDTO userDTO);

        // register user client
        public Task<ShowUserDTO> RegisterUserClient(RegisterUserDTO userDTO);

        // modify user name
        public Task<ShowUserDTO> ModifyUserName(string userName, ModifyUserNameDTO userNameDTO);

        // modify password
        public Task<ShowUserDTO> ModifyPassword(string userName, ModifyPassworDTO passwordDTO);

        // delete user
        public Task<ShowUserDTO> DeleteUser(string userName);

        public Task<string> Login(Login login);
    }
}

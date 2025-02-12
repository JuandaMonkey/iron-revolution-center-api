using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.DTOs.User;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private iUsersService _userService;
        public UsersController(iUsersService userService)
        {
            _userService = userService; 
        }

        #region ListUsers
        [HttpGet("List-Users")]
        public async Task<IActionResult> ListUsers()
        {
            try
            {
                var users = await _userService.ListUsers();

                if (users != null)
                    return Ok(users);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ListUsersByRole
        [HttpGet("List-Users-By-Role")]
        public async Task<IActionResult> ListUsersByRole([FromHeader] string roleID)
        {
            try
            {
                var users = await _userService.ListUsersByRole(roleID);

                if (users != null)
                    return Ok(users);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region GetUsersByUserName
        [HttpGet("Get-Users-By-User-Name")]
        public async Task<IActionResult> GetUsersByUserName([FromHeader] string userName)
        {
            try
            {
                var users = await _userService.GetUsersByUserName(userName);

                if (users != null)
                    return Ok(users);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterUser
        [HttpPost("Register-User")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO userDTO)
        {
            try
            {
                var client = await _userService.RegisterUser(userDTO);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterUserClient
        [HttpPost("Register-User-Client")]
        public async Task<IActionResult> RegisterUserClient([FromBody] RegisterUserDTO userDTO)
        {
            try
            {
                var client = await _userService.RegisterUserClient(userDTO);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyUserName
        [HttpPut("Modify-UserName")]
        public async Task<IActionResult> ModifyUserName([FromHeader] string userName, [FromBody] ModifyUserNameDTO userNameDTO)
        {
            try
            {
                var client = await _userService.ModifyUserName(userName, userNameDTO);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            }
            catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyPassword
        [HttpPut("Modify-Password")]
        public async Task<IActionResult> ModifyPassword([FromHeader] string userName, [FromBody] ModifyPassworDTO passwordDTO)
        {
            try
            {
                var client = await _userService.ModifyPassword(userName, passwordDTO);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            }
            catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteUser
        [HttpDelete("Delete-User")]
        public async Task<IActionResult> DeleteUser([FromHeader] string userName)
        {
            try
            {
                var client = await _userService.DeleteUser(userName);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            }
            catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

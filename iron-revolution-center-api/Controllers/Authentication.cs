using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.DTOs.User;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class Authentication : Controller
    {
        private readonly iUsersService _usersService;

        public Authentication(iUsersService userService)
        {
            _usersService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            try
            {
                var user = await _usersService.Login(login);

                if (user != null)
                    return Ok(user);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}

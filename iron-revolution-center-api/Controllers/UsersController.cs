using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.DTOs.Clients;
using iron_revolution_center_api.DTOs.Users;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        private IConfiguration _config;
        public UsersController(iUsersService userService, IConfiguration config)
        {
            _userService = userService; 
            _config = config;
        }

        #region RegisterUser
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTOs userDTO)
        {
            if (userDTO == null)
                return BadRequest("Los datos del usuario no pueden estar vacíos.");

            try
            {
                var user = await _userService.RegisterUser(userDTO);

                if (user != null)
                    return Ok(user);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region LoginUser
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromHeader] string userName, [FromHeader] string password)
        {
            if (string.IsNullOrEmpty(userName))
                return BadRequest("El nombre de usuario no puede estar vacío.");
            if (string.IsNullOrEmpty(password))
                return BadRequest("La contraseña no puede estar vacío.");

            try
            {
                var userToken = await _userService.GetUserByUsername(userName);
                var user = await _userService.LoginUser(userName, password);

                if (userToken == null || string.IsNullOrEmpty(userToken.Role))
                    return Unauthorized(new { message = "El usuario no tiene un rol válido." });

                var token = GenerateJwtToken(userToken.User_Name, userToken.Role);

                return Ok(new { token });
            } catch (Exception ex) {
                return Unauthorized(new { message = ex.Message });
            }
        }

        private string GenerateJwtToken(string username, string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                throw new ArgumentException("El nombre de usuario o el rol no pueden estar vacíos.");
            try
            {
                var secretKey = _config["JwtSettings:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                    throw new InvalidOperationException("La clave secreta no está configurada.");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _config["JwtSettings:Issuer"],
                    audience: _config["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            } catch (InvalidOperationException ex) {
                throw new InvalidOperationException("Error al generar el token: " + ex.Message);
            }
        }
        #endregion

        #region AssignClientByNIP
        [HttpPost("assign-client")]
        public async Task<IActionResult> assignClient([FromHeader] string user_name, [FromHeader] string NIP, [FromHeader] string password)
        {
            if (string.IsNullOrEmpty(user_name))
                return BadRequest("El nombre de usuario no puede estar vacío.");
            if (string.IsNullOrEmpty(NIP))
                return BadRequest("El NIP no puede estar vacío.");
            if (string.IsNullOrEmpty(password))
                return BadRequest("La contraseña no puede estar vacío.");

            try
            {
                var user = await _userService.AssignClient(user_name, NIP, password);

                return Ok(user);
            }
            catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

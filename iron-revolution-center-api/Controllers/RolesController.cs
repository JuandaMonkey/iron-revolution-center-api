using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.DTOs.Role;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : Controller
    {
        private iRolesInterface _roleService;
        public RolesController(iRolesInterface rolesService)
        {
            _roleService = rolesService;
        }

        #region ListRoles
        [HttpGet("Listar-Roles")]
        public async Task<IActionResult> ListRoles()
        {
            try
            {
                var roles = await _roleService.ListRoles();

                if (roles != null)
                    return Ok(roles);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region InsertRole
        [HttpPost("Insertar-Rol")]
        public async Task<IActionResult> InsertRole([FromBody] InsertRoleDTO roleDTO)
        {
            try
            {
                var role = await _roleService.InsertRole(roleDTO);

                if (role != null)
                    return Ok(role);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyRole
        [HttpPut("Modificar-Rol")]
        public async Task<IActionResult> ModifyRole([FromHeader] string roleID, [FromBody] ModifyRoleDTO roleDTO)
        {
            try
            {
                var role = await _roleService.ModifyRole(roleID, roleDTO);

                if (role != null)
                    return Ok(role);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteRole
        [HttpDelete("Eliminar-Rol")]
        public async Task<IActionResult> DeleteRole([FromHeader] string roleID)
        {
            try
            {
                var role = await _roleService.DeleteRole(roleID);

                if (role != null)
                    return Ok(role);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

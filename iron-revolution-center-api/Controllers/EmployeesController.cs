using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Staff;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : Controller
    {
        private iEmployeesService _employeesService;
        public EmployeesController(iEmployeesService employeesService)
        {
            _employeesService = employeesService;
        }

        #region ListEmployees
        [HttpGet("Listar-Empleados")]
        public async Task<IActionResult> ListEmployees()
        {
            try
            {
                var employees = await _employeesService.ListEmployees();

                if (employees != null)
                    return Ok(employees);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region GetEmployeeByNIP
        [HttpGet("Consultar-Empleado-Por-NIP")]
        public async Task<IActionResult> GetEmployeeByNIP([FromHeader] string NIP)
        {
            try
            {
                var employee = await _employeesService.GetEmployeeByNIP(NIP);

                if (employee != null)
                    return Ok(employee);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterEmployee
        [HttpPost("Registrar-Empleado")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeeDTO employeeDTO)
        {
            try
            {
                var employee = await _employeesService.RegisterEmployee(employeeDTO);

                if (employee != null)
                    return Ok(employee);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyEmployee
        [HttpPut("Modificar-Empleado")]
        public async Task<IActionResult> ModifyEmployee([FromHeader] string NIP, [FromBody] ModifyEmployeeDTO employeeDTO)
        {
            try
            {
                var employee = await _employeesService.ModifyEmployee(NIP, employeeDTO);

                if (employee != null)
                    return Ok(employee);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteStaff
        [HttpDelete("Eliminar-Empleado")]
        public async Task<IActionResult> DeleteStaff([FromHeader] string NIP)
        {
            try
            {
                var employee = await _employeesService.DeleteEmployee(NIP);

                if (employee != null)
                    return Ok(employee);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

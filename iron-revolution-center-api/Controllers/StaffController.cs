using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Staff;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : Controller
    {
        private iStaffService _staffService;
        public StaffController(iStaffService staffService)
        {
            _staffService = staffService;
        }

        #region ListStaff
        [HttpGet("List-Staff")]
        public async Task<IActionResult> ListStaff()
        {
            try
            {
                var staff = await _staffService.ListStaff();

                if (staff != null)
                    return Ok(staff);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ListStaffByBranchOffice
        [HttpGet("List-Staff-By-Branch-Office")]
        public async Task<IActionResult> ListStaffByBranchOffice([FromHeader] string branchID)
        {
            try
            {
                var staff = await _staffService.ListStaffByBranchOffice(branchID);

                if (staff != null)
                    return Ok(staff);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region GetStaffByNIP
        [HttpGet("Get-Staff-By-NIP")]
        public async Task<IActionResult> GetStaffByNIP([FromHeader] string NIP)
        {
            try
            {
                var staff = await _staffService.GetStaffByNIP(NIP);

                if (staff != null)
                    return Ok(staff);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterStaff
        [HttpPost("Register-Staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDTO staffDTO)
        {
            try
            {
                var staff = await _staffService.RegisterStaff(staffDTO);

                if (staff != null)
                    return Ok(staff);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyStaff
        [HttpPut("Modify-Staff")]
        public async Task<IActionResult> ModifyStaff([FromHeader] string NIP, [FromBody] ModifyStaffDTO staffDTO)
        {
            try
            {
                var staff = await _staffService.ModifyStaff(NIP, staffDTO);

                if (staff != null)
                    return Ok(staff);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteStaff
        [HttpDelete("Delete-Staff")]
        public async Task<IActionResult> DeleteStaff([FromHeader] string NIP)
        {
            try
            {
                var staff = await _staffService.DeleteStaff(NIP);

                if (staff != null)
                    return Ok(staff);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

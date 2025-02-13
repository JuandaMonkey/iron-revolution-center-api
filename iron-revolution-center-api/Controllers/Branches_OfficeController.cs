using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Branch_Office;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Branches_OfficeController : Controller
    {
        private readonly iBranches_OfficeService _branchesOfficeService;

        public Branches_OfficeController(iBranches_OfficeService branchesOfficeService)
        {
            _branchesOfficeService = branchesOfficeService;
        }

        #region ListBranch_Office
        [HttpGet("List-Branch-Office")]
        public async Task<IActionResult> ListBranch_Office()
        {
            try
            {
                var branches = await _branchesOfficeService.ListBranch_Office();

                if (branches != null)
                    return Ok(branches);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterBranch_Office
        [HttpPost("Register-Branch-Office")]
        public async Task<IActionResult> RegisterBranch_Office([FromBody] InsertBranche_OfficeDTO branchOfficeDTO)
        {
            try
            {
                var branch = await _branchesOfficeService.RegisterBranch_Office(branchOfficeDTO);

                if (branch != null)
                    return Ok(branch);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyBranch_Office
        [HttpPut("Modify-Branch-Office")]
        public async Task<IActionResult> ModifyBranch_Office([FromHeader] string branchOfficeID, [FromBody] ModifyBranche_OfficeDTO branchOfficeDTO)
        {
            try
            {
                var branch = await _branchesOfficeService.ModifyBranch_Office(branchOfficeID, branchOfficeDTO);
               
                if (branch != null)
                    return Ok(branch);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteBranch_Office
        [HttpDelete("Delete-Branch-Office")]
        public async Task<IActionResult> DeleteBranch_Office([FromHeader] string branchOfficeID)
        {
            try
            {
                var branch = await _branchesOfficeService.DeleteBranch_Office(branchOfficeID);
               
                if (branch != null)
                    return Ok(branch);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

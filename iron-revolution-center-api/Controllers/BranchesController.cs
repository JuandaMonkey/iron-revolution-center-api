using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Branch_Office;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : Controller
    {
        private readonly iBranchesService _branchesService;

        public BranchesController(iBranchesService branchesService)
        {
            _branchesService = branchesService;
        }

        #region ListBranch
        [HttpGet("Listar-Sucursales")]
        public async Task<IActionResult> ListBranches()
        {
            try
            {
                var branches = await _branchesService.ListBranches();

                if (branches != null)
                    return Ok(branches);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region InsertBranch
        [HttpPost("Insertar-Sucursal")]
        public async Task<IActionResult> RegisterBranch([FromBody] InsertBranchDTO branchDTO)
        {
            try
            {
                var branch = await _branchesService.RegisterBranch(branchDTO);

                if (branch != null)
                    return Ok(branch);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyBranch
        [HttpPut("Modificar-Sucursal")]
        public async Task<IActionResult> ModifyBranch([FromHeader] string branchId, [FromBody] ModifyBranchDTO branchDTO)
        {
            try
            {
                var branch = await _branchesService.ModifyBranch(branchId, branchDTO);
               
                if (branch != null)
                    return Ok(branch);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteBranch
        [HttpDelete("Eliminar-Sucursal")]
        public async Task<IActionResult> DeleteBranch([FromHeader] string branchId)
        {
            try
            {
                var branch = await _branchesService.DeleteBranch(branchId);
               
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

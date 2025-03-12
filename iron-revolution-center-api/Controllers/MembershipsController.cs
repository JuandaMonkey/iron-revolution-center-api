using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Membership;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.Eventing.Reader;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipsController : Controller
    {
        private iMembershipsService _membershipService;
        public MembershipsController(iMembershipsService membershipService)
        {
            _membershipService = membershipService;
        }

        #region ListMemberships
        [HttpGet("Listar-Membresías")]
        public async Task<IActionResult> ListMemberships()
        {
            try
            {
                var memberships = await _membershipService.ListMemberships();

                if (memberships != null)
                    return Ok(memberships);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region InsertMembership
        [HttpPost("Insertar-Membresía")]
        public async Task<IActionResult> InsertMembership([FromBody] InsertMembershipDTO membershipDTO)
        {
            try
            {
                var membership = await _membershipService.InsertMembership(membershipDTO);

                if (membership != null)
                    return Ok(membership);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }
        #endregion

        #region ModifyMembership
        [HttpPut("Modificar-Membresía")]
        public async Task<IActionResult> ModifyMembership([FromHeader] string membershipID, [FromBody] ModifyMembershipDTO membershipDTO)
        {
            try
            {
                var membership = await _membershipService.ModifyMembership(membershipID, membershipDTO);

                if (membership != null)
                    return Ok(membership);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteMembership
        [HttpDelete("Eliminar-Membresía")]
        public async Task<IActionResult> DeleteMembership([FromHeader] string membershipID)
        {
            try
            {
                var membership = await _membershipService.DeleteMembership(membershipID);

                if (membership != null)
                    return Ok(membership);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"{ex.Message}");
            }
        }
        #endregion

        #region AssignMembership
        [HttpPut("Asignar-Membresía")]
        public async Task<IActionResult> AssignMembership([FromHeader] string NIP, [FromHeader] string membershipID)
        {
            try
            {
                var membership = await _membershipService.AssignMembership(NIP, membershipID);

                if (membership != null)
                    return Ok(membership);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"{ex.Message}");
            }
        }
        #endregion
    }
}

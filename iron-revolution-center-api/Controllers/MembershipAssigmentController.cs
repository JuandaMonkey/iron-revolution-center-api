using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.DTOs.Staff;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipAssigmentController : Controller
    {
        private iMembershipAssigmentService _membershipAssigment;
        public MembershipAssigmentController(iMembershipAssigmentService membershipAssigment)
        {
            _membershipAssigment = membershipAssigment;
        }

        #region AssignMembership
        [HttpPost("Assign-Membership")]
        public async Task<IActionResult> AssignMembership([FromHeader] string NIP, [FromHeader] string membershipID)
        {
            try
            {
                var client = await _membershipAssigment.AssignMembership(NIP, membershipID);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region GetMembershipDetails
        [HttpPost("Get-Membership-Details")]
        public async Task<IActionResult> GetMembershipDetails([FromHeader] string NIP)
        {
            try
            {
                var client = await _membershipAssigment.GetMembershipDetails(NIP);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

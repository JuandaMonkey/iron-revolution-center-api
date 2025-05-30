﻿using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Membership;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

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
        [HttpGet("ListarMembresias")]
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
        [HttpPost("InsertarMembresia")]
        public async Task<IActionResult> InsertMembership([FromBody] newMembershipDTO membershipDTO)
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
        [HttpPut("ModificarMembresia")]
        public async Task<IActionResult> ModifyMembership(string membershipID, [FromBody] ModifyMembershipDTO membershipDTO)
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
        [HttpDelete("EliminarMembresia")]
        public async Task<IActionResult> DeleteMembership(string membershipID)
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
        [HttpPut("AsignarMembresia")]
        public async Task<IActionResult> AssignMembership(string NIP, string membershipID)
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

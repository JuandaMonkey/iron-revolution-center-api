using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Activity_CenterController : Controller
    {
        private iActivity_CenterService _activity_CenterService;
        public Activity_CenterController(iActivity_CenterService activityCenter)
        {
            _activity_CenterService = activityCenter;
        }

        #region ListActivityCenter
        [HttpGet("ListarCentroDeActividad")]
        public async Task<IActionResult> ListActivityCenter(string? branchId, string startDay, string endDay)
        {
            try
            {
                DateTime parsedStartDay = DateTime.ParseExact(startDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime parsedEndDay = DateTime.ParseExact(endDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var activity = await _activity_CenterService.ListActivityCenter(branchId, parsedStartDay, parsedEndDay);

                if (activity != null)
                    return Ok(activity);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterEntry
        [HttpPost("Registrar-Entrada")]
        public async Task<IActionResult> RegisterEntry(string NIP, string securityKey, [FromHeader] string branchiD)
        {
            try
            {
                var activity = await _activity_CenterService.RegisterEntry(NIP, securityKey, branchiD);

                if (activity == true)
                    return Ok(activity);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterExit
        [HttpPut("Registrar-Salida")]
        public async Task<IActionResult> RegisterExit(string NIP)
        {
            try
            {
                var activity = await _activity_CenterService.RegisterExit(NIP);

                if (activity == true)
                    return Ok(activity);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

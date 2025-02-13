using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        #region ListActivity
        [HttpGet("List-Activity")]
        public async Task<IActionResult> ListActivity()
        {
            try
            {
                var activity = await _activity_CenterService.ListActivity();

                if (activity != null)
                    return Ok(activity);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterEntry
        [HttpPost("Register-Entry")]
        public async Task<IActionResult> RegisterEntry([FromHeader] string NIP, [FromHeader] string branchOffice)
        {
            try
            {
                var activity = await _activity_CenterService.RegisterEntry(NIP, branchOffice);

                if (activity != null)
                    return Ok(activity);
                else
                    return NoContent();
            } catch (Exception ex) { 
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterExit
        [HttpPost("RegisterExit")]
        public async Task<IActionResult> RegisterExit([FromHeader] string NIP)
        {
            try
            {
                var activity = await _activity_CenterService.RegisterExit(NIP);

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
    }
}

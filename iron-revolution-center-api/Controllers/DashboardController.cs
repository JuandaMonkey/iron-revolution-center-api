using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Interfaces;
using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly iDashboardService _dashboardService;

        public DashboardController(iDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        #region Dashboard
        [HttpGet("InformacionDashboard/{branchId}")]
        public async Task<IActionResult> Dashboard(string branchId)
        {
            try
            {
                var dashboard = await _dashboardService.Dashboard(branchId);

                if (dashboard != null)
                    return Ok(dashboard);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}

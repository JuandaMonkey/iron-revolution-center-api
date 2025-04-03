using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Interfaces;
using iron_revolution_center_api.Data.Service;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : Controller
    {
        private iStatisticsService _statisticsService;
        public StatisticsController(iStatisticsService Statistics)
        {
            _statisticsService = Statistics;
        }

        [HttpGet("ClientesRegistrados")]
        public async Task<IActionResult> getRegisteredClients(string? branchId)
        {
            try
            {
                var count = await _statisticsService.getRegisteredClients(branchId);

                if (count != null)
                    return Ok(count);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("ClientesActivos")]
        public async Task<IActionResult> getActiveClients(string? branchId)
        {
            try
            {
                var count = await _statisticsService.getActiveClients(branchId);

                if (count != null)
                    return Ok(count);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        [HttpGet("EmpleadosRegistrados")]
        public async Task<IActionResult> getRegisteredEmployees(string? branchId)
        {
            try
            {
                var count = await _statisticsService.getRegisteredEmployees(branchId);

                if (count != null)
                    return Ok(count);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}

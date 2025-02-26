using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : Controller
    {
        private iClientsService _clientService;
        public ClientsController(iClientsService clientService)
        {
            _clientService = clientService;
        }

        #region ListsClients
        [HttpGet("Listar-Clientes")]
        public async Task<IActionResult> ListClients()
        {
            try
            {
                var clients = await _clientService.ListClients();

                if (clients != null)
                    return Ok(clients);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region GetClientByNIP
        [HttpGet("Consultar-Cliente-Por-NIP")]
        public async Task<IActionResult> GetClientByNIP([FromHeader] string NIP)
        {
            try
            {
                var clients = await _clientService.GetClientByNIP(NIP);

                if (clients != null)
                    return Ok(clients);
                else
                    return NoContent();
            } catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterClient
        [HttpPost("Registrar-Cliente")]
        public async Task<IActionResult> RegisterClient([FromForm] RegisterClientDTO clientDTO)
        {
            try
            {
                var client = await _clientService.RegisterClient(clientDTO);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region ModifyClient
        [HttpPut("Modificar-Cliente")]
        public async Task<IActionResult> ModifyClient([FromHeader] string NIP, [FromBody] ModifyClientDTO clientDTO)
        {
            try
            {
                var client = await _clientService.ModifyClient(NIP, clientDTO);

                if (client != null)
                    return Ok(client);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region DeleteClient
        [HttpDelete("Eliminar-Cliente")]
        public async Task<IActionResult> DeleteClient([FromHeader] string NIP)
        {
            try
            {
                var client = await _clientService.DeleteClient(NIP);

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

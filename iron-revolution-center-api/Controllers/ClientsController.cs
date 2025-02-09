using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Clients;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : Controller
    {
        private iClientsService _clientService;
        public ClientsController(iClientsService clientService) => _clientService = clientService;

        #region ListsClients
        [HttpGet]
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

        #region SearchClientByNIP
        [HttpGet("{NIP}")]
        public async Task<IActionResult> getClientByNIP(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                return BadRequest("El NIP no puede estar vacío.");

            try
            {
                var clients = await _clientService.GetClientByNIP(NIP);

                if (clients != null)
                    return Ok(clients);
                else
                    return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region register_client
        [HttpPost]
        public async Task<IActionResult> registerClient([FromBody] RegisterClientsDTos clientDTO)
        {
            if (clientDTO == null)
                return BadRequest("Los datos del cliente no pueden estar vacíos.");

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

        #region modify_client
        [HttpPut("{NIP}")]
        public async Task<IActionResult> modifyClient(string NIP, [FromBody] ModifyClientDTOs clientDTO)
        {
            if (string.IsNullOrEmpty(NIP))
                return BadRequest("El NIP no puede estar vacío.");
            if (clientDTO == null)
                return BadRequest("Los datos del cliente no pueden estar vacíos.");

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

        #region delete_client
        [HttpDelete("{NIP}")]
        public async Task<IActionResult> deleteClient(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                BadRequest("El NIP no puede estar vacío.");

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

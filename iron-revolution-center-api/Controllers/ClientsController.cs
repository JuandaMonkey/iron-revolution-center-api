using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

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
        [HttpGet("ListarClientes")]
        public async Task<IActionResult> ListClients(string? membershipId, string startDay, string endDay)
        {
            try
            {
                DateOnly parsedStartDay = DateOnly.ParseExact(startDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateOnly parsedEndDay = DateOnly.ParseExact(endDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var clients = await _clientService.ListClients(membershipId, parsedStartDay, parsedEndDay);

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
        [HttpGet("ConsultarClientePorNIP")]
        public async Task<IActionResult> GetClientByNIP(string NIP)
        {
            try
            {
                var clients = await _clientService.GetClientByNIP(NIP);

                if (clients != null)
                    return Ok(clients);
                else
                    return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion

        #region RegisterClient
        [HttpPost("RegistrarCliente")]
        public async Task<IActionResult> RegisterClient([FromBody] newClientModel clientDTO)
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
        [HttpPut("ModificarCliente")]
        public async Task<IActionResult> ModifyClient(string NIP, [FromBody] modifyClientModel clientDTO)
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
        [HttpDelete("EliminarCliente")]
        public async Task<IActionResult> DeleteClient(string NIP)
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

        #region ModifyClient
        [HttpPut("Generar-Nueva-Clave-De-Seguridad")]
        public async Task<IActionResult> GenerateSegurityKey([FromHeader] string NIP)
        {
            try
            {
                var client = await _clientService.GenerateSegurityKey(NIP);

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

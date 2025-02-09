using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iron_revolution_center_api.DTOs.Clients;
using iron_revolution_center_api.Models;
using MongoDB.Driver;

namespace iron_revolution_center_api.Data.Interface
{
    // interface for the ClientsService
    public interface iClientsService
    {
        // list of clients
        public Task<IEnumerable<ClientsModel>> ListClients();

        // get a client by nip
        public Task<IEnumerable<ClientsModel>> GetClientByNIP(string NIP);

        // register a client
        public Task<RegisterClientsDTos> RegisterClient(RegisterClientsDTos RegisterClientsDTos);

        // modify a client
        public Task<ClientsModel> ModifyClient(string NIP, ModifyClientDTOs ModifyClientDTOs);

        // delete a client
        public Task<ClientsModel> DeleteClient(string NIP);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace iron_revolution_center_api.Data.Interface
{
    // interface for the ClientsService
    public interface iClientsService
    {
        // list of clients
        public Task<IEnumerable<ClientsModel>> ListClients(string membershipId, DateOnly startDay, DateOnly endDay);

        // get client by nip
        public Task<ClientsModel> GetClientByNIP(string NIP);

        // register client
        public Task<ClientsModel> RegisterClient(newClientModel clientDTO);

        // modify client
        public Task<ClientsModel> ModifyClient(string NIP, modifyClientModel clientDTO);

        // delete client
        public Task<ClientsModel> DeleteClient(string NIP);

        // generate a segurity key
        public Task<ClientsModel> GenerateSegurityKey(string NIP);
    }
}

using iron_revolution_center_api.DTOs.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interfaces
{
    public interface iStatisticsService
    {
        public Task<registeredClients> getRegisteredClients(string branchId);

        public Task<activeClientsDTO> getActiveClients(string branchId);

        public Task<registeredEmployees> getRegisteredEmployees(string branchId);
    }
}

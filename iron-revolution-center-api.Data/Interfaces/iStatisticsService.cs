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

        public Task<activeClients> getActiveClients(string branchId);

        public Task<registeredEmployees> getRegisteredEmployees(string branchId);

        public Task<IEnumerable<branchesCount>> getBranchesCount();

        public Task<branchesCount> getMostFrecuentedBranch(string branchId);

        public Task<IEnumerable<membershipsCount>> getMostPopularMemberships(string branchId);
    }
}

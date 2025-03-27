using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interfaces
{
    public interface iDashboardService
    {
        public Task<DashboardModel> Dashboard(string branchId);
    }
}

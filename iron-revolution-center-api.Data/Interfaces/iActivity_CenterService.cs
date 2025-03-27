using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   

namespace iron_revolution_center_api.Data.Interface
{
    public interface iActivity_CenterService
    {
        public Task<IEnumerable<Activity_CenterModel>> ListActivityCenter(string branchId, DateTime startDay, DateTime endDay);

        public Task<bool> RegisterEntry(string NIP, string branchId);

        public Task<bool> RegisterExit(string NIP);
    }
}

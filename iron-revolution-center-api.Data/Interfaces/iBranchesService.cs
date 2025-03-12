using iron_revolution_center_api.DTOs.Branch_Office;
using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    public interface iBranchesService
    {
        // list of branches office
        public Task<IEnumerable<BranchesModel>> ListBranches();

        // register branch office
        public Task<InsertBranchDTO> RegisterBranch(InsertBranchDTO branchDTO);

        // modify brancg office
        public Task<BranchesModel> ModifyBranch(string branchId, ModifyBranchDTO branchDTO);

        // delete branch office
        public Task<BranchesModel> DeleteBranch(string branchId);
    }
}

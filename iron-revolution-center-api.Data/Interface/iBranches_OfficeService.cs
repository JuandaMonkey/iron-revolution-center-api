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
    public interface iBranches_OfficeService
    {
        // list of branches office
        public Task<IEnumerable<Branches_OfficeModel>> ListBranch_Office();

        // register branch office
        public Task<InsertBranche_OfficeDTO> RegisterBranch_Office(InsertBranche_OfficeDTO branchOfficeDTO);

        // modify brancg office
        public Task<Branches_OfficeModel> ModifyBranch_Office(string branchOfficeID, ModifyBranche_OfficeDTO branchOfficeDTO);

        // delete branch office
        public Task<Branches_OfficeModel> DeleteBranch_Office(string branchOfficeID);
    }
}

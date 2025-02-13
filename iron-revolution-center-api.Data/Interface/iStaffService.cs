using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    public interface iStaffService
    {
        // list of staff
        public Task<IEnumerable<StaffModel>> ListStaff();

        // list of staff by branch office
        public Task<IEnumerable<StaffModel>> ListStaffByBranchOffice(string branchID);

        // get staff by nip
        public Task<IEnumerable<StaffModel>> GetStaffByNIP(string NIP);

        // register staff
        public Task<RegisterStaffDTO> RegisterStaff(RegisterStaffDTO staffDTO);

        // modify staff
        public Task<StaffModel> ModifyStaff(string NIP, ModifyStaffDTO staffDTO);

        // delete staff
        public Task<StaffModel> DeleteStaff(string NIP);
    }
}

using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.DTOs.Membership;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    // interface for the MembershipsService
    public interface iMembershipsService
    {
        // list of memberships
        public Task<IEnumerable<MembershipsModel>> ListMemberships();

        // get membership by identification
        public Task<IEnumerable<MembershipsModel>> GetMembershipByID(string membershipID);

        // insert membership
        public Task<InsertMembershipDTO> InsertMembership(InsertMembershipDTO membershipDTo);

        // modify membership
        public Task<MembershipsModel> ModifyMembership(string membershipID, ModifyMembershipDTO membershipDTO);

        // delete membership
        public Task<MembershipsModel> DeleteMembership(string membershipID);
    }
}

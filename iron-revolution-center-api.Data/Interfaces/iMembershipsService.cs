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

        // insert membership
        public Task<InsertMembershipDTO> InsertMembership(newMembershipDTO membershipDTo);

        // modify membership
        public Task<MembershipsModel> ModifyMembership(string membershipId, ModifyMembershipDTO membershipDTO);

        // delete membership
        public Task<MembershipsModel> DeleteMembership(string membershipId);

        // assing membership 
        public Task<ClientsModel> AssignMembership(string NIP, string membershipId);
    }
}

using iron_revolution_center_api.DTOs.MembershipAssignment;
using iron_revolution_center_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Interface
{
    public interface iMembershipAssigmentService
    {
        // assign membership
        public Task<MembershipDetailDTO> AssignMembership(string NIP, string membershipID);

        // get membership details
        public Task<MembershipDetailDTO> GetMembershipDetails(string NIP);
    }
}

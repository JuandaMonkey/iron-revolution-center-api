using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.MembershipAssignment;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Service
{
    public class MembershipAssigmentService : iMembershipAssigmentService
    {
        #region MongoDB Configuration
        private readonly IMongoCollection<MembershipAssignmentModel> _membershipAssignmentsCollection;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;
        private readonly IMongoCollection<MembershipsModel> _membershipsCollection;

        public MembershipAssigmentService(IMongoDatabase database)
        {
            _membershipAssignmentsCollection = database.GetCollection<MembershipAssignmentModel>("MembershipAssignments");
            _clientsCollection = database.GetCollection<ClientsModel>("Clients");
            _membershipsCollection = database.GetCollection<MembershipsModel>("Memberships");
        }
        #endregion

        #region Validations
        private async Task<bool> ValidateClientNIP(string NIP)
        {
            try
            {
                // check client 
                var clientExists = await _clientsCollection
                    .CountDocumentsAsync(client => client.NIP == NIP);

                // validate existence
                return clientExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> ValidateMembershipExists(string membershipID)
        {
            try
            {
                // check membership
                var membershipExists = await _membershipsCollection
                    .CountDocumentsAsync(membership => membership.Membership_ID == membershipID);

                // validate existence
                return membershipExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        #endregion

        #region AssignMembership
        public async Task<MembershipDetailDTO> AssignMembership(string NIP, string membershipID)
        {
            if (string.IsNullOrEmpty(NIP)) // field validation
                throw new ArgumentException($"El NIP no puede estar vacío. {nameof(NIP)}");
            if (!await ValidateClientNIP(NIP)) // field validation
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            if (!await ValidateMembershipExists(membershipID)) // field validation
                throw new ArgumentException($"La membresía con ID: {membershipID} no existe.");
            try
            {
                // membership
                var membership = await _membershipsCollection
                    .Find(membership => membership.Membership_ID == membershipID)
                    .FirstOrDefaultAsync();

                // client
                var membershipClient = await _membershipAssignmentsCollection
                    .Find(client => client.NIP == NIP)
                    .FirstOrDefaultAsync();

                if (membershipClient == null || membershipClient.Membership_ID == null)
                {
                    // assignament
                    var assignment = new MembershipAssignmentModel
                    {
                        NIP = NIP,
                        Membership_ID = membershipID,
                        Start_Date = DateTime.UtcNow,
                        End_Date = DateTime.UtcNow.AddDays(membership.Duration),
                        Status = true
                    };

                    // check if is not null
                    if (assignment == null)
                        throw new ArgumentException("Asignación fallida.");

                    // assign
                    await _membershipAssignmentsCollection.InsertOneAsync(assignment);
                }
                else
                {
                    // filter
                    var filter = Builders<MembershipAssignmentModel>.Filter.Eq(assign => assign.NIP, NIP);
                    // update
                    var update = Builders<MembershipAssignmentModel>.Update
                        .Set(assign => assign.Membership_ID, membershipID)
                        .Set(assign => assign.Start_Date, DateTime.UtcNow)
                        .Set(assign => assign.End_Date, DateTime.UtcNow.AddDays(membership.Duration))
                        .Set(assign => assign.Status, true);

                    // update
                    await _membershipAssignmentsCollection.UpdateOneAsync(filter, update);
                }

                // find 
                var result = await GetMembershipDetails(NIP);

                // full info client
                return result;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar trabajador. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region GetMembershipDetails
        public async Task<MembershipDetailDTO> GetMembershipDetails(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                throw new ArgumentException($"El NIP no puede estar vacío. {nameof(NIP)}");
            try
            {
                // details info
                var assignment = await _membershipAssignmentsCollection
                    .Find(assigment => assigment.NIP == NIP)
                    .FirstOrDefaultAsync();
                if (assignment == null) // check if is not null
                    throw new ArgumentException("No se encontró la asignación de membresía.");

                // client
                var client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .FirstOrDefaultAsync();
                if (client == null) // check if is not null
                    throw new ArgumentException("No se encontró el cliente.");

                // membership
                var membership = await _membershipsCollection
                    .Find(m => m.Membership_ID == assignment.Membership_ID)
                    .FirstOrDefaultAsync();
                if (membership == null) // check if is not null
                    throw new ArgumentException("No se encontró la membresía.");

                // full info
                var membershipDetail = new MembershipDetailDTO
                {
                    Client = client,
                    Membership = membership,
                    Start_Date = assignment.Start_Date,
                    End_Date = assignment.End_Date,
                    Status = assignment.Status
                };

                // client full info
                return membershipDetail;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar cliente. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}

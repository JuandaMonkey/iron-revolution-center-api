using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Membership;
using iron_revolution_center_api.DTOs.Users;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Service
{
    public class MembershipsService : iMembershipsService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<MembershipsModel> _membershipCollection;
        private readonly IMongoCollection<InsertMembershipDTO> _insertMembershipCollection;

        // method to exclude _id field
        private static ProjectionDefinition<ClientsModel> ExcludeIdProjection()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }

        public MembershipsService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _membershipCollection = _mongoDatabase.GetCollection<MembershipsModel>("Memberships");
            _insertMembershipCollection = _mongoDatabase.GetCollection<InsertMembershipDTO>("Memberships");
        }
        #endregion

        #region Validations
        private async Task<bool> ValidateMembershipName(string name)
        {
            try
            {
                // check membership name
                var nameIsAlreadyUsed = await _membershipCollection
                    .CountDocumentsAsync(membership => membership.Name == name);

                // validate existence
                return nameIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> ValidateMembershipID(string membershipID)
        {
            try
            {
                // check membership identification
                var IDExists = await _membershipCollection
                    .CountDocumentsAsync(membership => membership.Membership_ID == membershipID);

                // validate existence
                return IDExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        #endregion

        #region ListMemberships
        public async Task<IEnumerable<MembershipsModel>> ListMemberships()
        {
            try
            {
                // get the collection
                return await _membershipCollection
                    .Find(FilterDefinition<MembershipsModel>.Empty)
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException("Error al mostrar membresías.", ex);
            }
        }
        #endregion

        #region GetMembershipByID
        public async Task<IEnumerable<MembershipsModel>> GetMembershipByID(string membershipID)
        {
            if (string.IsNullOrEmpty(membershipID)) // field verification
                throw new ArgumentException($"El ID no puede estar vacío. {nameof(membershipID)}");
            try
            {
                // get the collection
                return await _membershipCollection
                    .Find(membership => membership.Membership_ID == membershipID)
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error: {ex}");
            } catch (ArgumentException ex)
            {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region InsertMembership
        public async Task<InsertMembershipDTO> InsertMembership(InsertMembershipDTO membershipDTO)
        {
            if (string.IsNullOrEmpty(membershipDTO.Name)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(membershipDTO.Name)}");
            if (membershipDTO.Duration < 0) // field verification
                throw new ArgumentException($"La duracion no puede ser negativo. {nameof(membershipDTO.Name)}");
            try
            {
                // generate a unique identification
                string id;
                string num;
                do
                {
                    num = new Random().Next(1, 1000).ToString("D3");
                    id = $"M{num}";
                }
                // check if identification is already used
                while (await _membershipCollection.Find(membership => membership.Membership_ID == id).AnyAsync());

                if (await ValidateMembershipName(membershipDTO.Name))
                    throw new Exception($"El nombre de membresía esta en uso.");

                // insert model
                var newMembership = new InsertMembershipDTO
                {
                    Membership_ID = id,
                    Name = membershipDTO.Name,
                    Duration = membershipDTO.Duration
                };

                // check if is not null
                if (newMembership == null)
                    throw new Exception($"Inserción fallida.");

                // insert
                await _insertMembershipCollection.InsertOneAsync(newMembership);

                // membership
                return newMembership;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error: {ex}");
            } catch (ArgumentException ex) {
                // in case of error 
                throw new ArgumentException($"Error: {ex}");
            } catch (Exception ex) {
                // in case of error
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyMembership
        public async Task<MembershipsModel> ModifyMembership(string membershipID, ModifyMembershipDTO membershipDTO)
        {
            if (string.IsNullOrEmpty(membershipID)) // field verification
                throw new ArgumentException($"El ID no puede estar vacío. {nameof(membershipID)}");
            if (membershipDTO == null) // field verification
                throw new ArgumentException($"No puede estar vacío. {nameof(membershipDTO)}");
            try
            {
                // check if exists
                if (await ValidateMembershipID(membershipID) == false)
                    throw new Exception($"El ID {membershipID} no existe.");

                // create update definitions
                var updateBuilder = Builders<MembershipsModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<MembershipsModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(membershipDTO.Name)) // name
                    updateDefinitions.Add(updateBuilder
                                     .Set(membership => membership.Name, membershipDTO.Name));
                if (membershipDTO.Duration == null) // duration
                    updateDefinitions.Add(updateBuilder
                                     .Set(membership => membership.Duration, membershipDTO.Duration));

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<MembershipsModel>
                    .Filter
                    .Eq(membership => membership.Membership_ID, membershipID);
                var update = await _membershipCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new InvalidOperationException("Error al modificar membresía.");

                // find membership
                MembershipsModel membership = await _membershipCollection
                    .Find(filter)
                    .FirstOrDefaultAsync();

                // membership
                return membership;
            } catch (MongoException ex) {
                // in case of erro
                throw new InvalidOperationException($"Error: {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            } catch (Exception ex) {
                // in case of error
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteMembership
        public Task<MembershipsModel> DeleteMembership(string membershipID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

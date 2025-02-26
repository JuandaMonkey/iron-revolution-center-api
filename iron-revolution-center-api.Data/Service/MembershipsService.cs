﻿using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Membership;
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
        private readonly IMongoCollection<ClientsModel> _clientsCollection;

        // method to exclude _id field
        private static ProjectionDefinition<MembershipsModel> ExcludeIdProjection()
        {
            return Builders<MembershipsModel>.Projection.Exclude("_id");
        }

        public MembershipsService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _membershipCollection = _mongoDatabase.GetCollection<MembershipsModel>("Memberships");
            _insertMembershipCollection = _mongoDatabase.GetCollection<InsertMembershipDTO>("Memberships");
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
        }
        #endregion

        #region Validations
        private async Task<bool> ValidateMembershipName(string name)
        {
            try
            {
                // check membership name
                var nameIsAlreadyUsed = await _membershipCollection
                    .CountDocumentsAsync(membership => membership.Nombre == name);

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
                    .CountDocumentsAsync(membership => membership.Membresia_ID == membershipID);

                // validate existence
                return IDExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> ValidateClientNIP(string NIP)
        {
            try
            {
                // check client 
                var clientExists = await _clientsCollection
                    .CountDocumentsAsync(client => client.NIP == NIP);

                // validate existence
                return clientExists > 0;
            }
            catch
            {
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
                    .Project<MembershipsModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar membresías. {ex}");
            }
        }
        #endregion

        #region InsertMembership
        public async Task<InsertMembershipDTO> InsertMembership(InsertMembershipDTO membershipDTO)
        {
            if (string.IsNullOrEmpty(membershipDTO.Nombre)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(membershipDTO.Nombre)}");
            if (await ValidateMembershipName(membershipDTO.Nombre)) // field verification
                throw new ArgumentException($"El nombre de membresía esta en uso.");
            if (membershipDTO.Duracion <= 0) // field verification
                throw new ArgumentException($"La duracion no puede ser menor o igual a 0. {nameof(membershipDTO.Nombre)}");
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
                while (await ValidateMembershipID(id) == true);

                // insert membership
                var newMembership = new InsertMembershipDTO
                {
                    Membresia_ID = id,
                    Nombre = membershipDTO.Nombre,
                    Duracion = membershipDTO.Duracion
                };

                // check if is not null
                if (newMembership == null)
                    throw new ArgumentException($"Inserción fallida.");

                // insert
                await _insertMembershipCollection.InsertOneAsync(newMembership);

                // membership
                return newMembership;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al insertar membresía. {ex}");
            } catch (ArgumentException ex) {
                // in case of error 
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyMembership
        public async Task<MembershipsModel> ModifyMembership(string membershipID, ModifyMembershipDTO membershipDTO)
        {
            if (string.IsNullOrEmpty(membershipID)) // field verification
                throw new ArgumentException($"El ID no puede estar vacío. {nameof(membershipID)}");
            if (!await ValidateMembershipID(membershipID)) // field verification
                throw new ArgumentException($"El ID {membershipID} no existe.");
            if (membershipDTO.Duracion <= 0 ) // field verification
                throw new ArgumentException($"La duracion no puede ser menor o igual a 0. {nameof(membershipDTO.Duracion)}");
            try
            {
                // create update definitions
                var updateBuilder = Builders<MembershipsModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<MembershipsModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(membershipDTO.Nombre)) // name
                    updateDefinitions.Add(updateBuilder
                                     .Set(membership => membership.Nombre, membershipDTO.Nombre));
                if (membershipDTO.Duracion.HasValue) // duration
                    updateDefinitions.Add(updateBuilder
                                     .Set(membership => membership.Duracion, membershipDTO.Duracion));

                // verification
                if (!updateDefinitions.Any())
                    throw new ArgumentException("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<MembershipsModel>
                    .Filter
                    .Eq(membership => membership.Membresia_ID, membershipID);
                var update = await _membershipCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar membresía.");

                // find membership
                MembershipsModel membership = await _membershipCollection
                    .Find(filter)
                    .Project<MembershipsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // membership
                return membership;
            } catch (MongoException ex) {
                // in case of erro
                throw new InvalidOperationException($"Error al modificar membresía con el ID: {membershipID}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteMembership
        public async Task<MembershipsModel> DeleteMembership(string membershipID)
        {
            if (string.IsNullOrEmpty(membershipID)) // field verification
                throw new ArgumentException($"El ID no puede estar vacío. {nameof(membershipID)}");
            if (!await ValidateMembershipID(membershipID)) // field verification
                throw new ArgumentException($"El ID: {membershipID} no existe.");
            try
            {
                // membership
                MembershipsModel membership = await _membershipCollection
                    .Find(membership => membership.Membresia_ID == membershipID)
                    .Project<MembershipsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // delete membership
                var delete = await _membershipCollection
                    .DeleteOneAsync(membership => membership.Membresia_ID == membershipID);

                // check 
                if (delete.DeletedCount == 0)
                    throw new ArgumentException("Error al eliminar membresía.");

                // return membership
                return membership;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al eliminar al membresía con el ID: {membershipID}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region AssignMembership
        public async Task<ClientsModel> AssignMembership(string NIP, string membershipID)
        {
            if (string.IsNullOrEmpty(NIP)) // field validation
                throw new ArgumentException($"El NIP no puede estar vacío. {nameof(NIP)}");
            if (!await ValidateClientNIP(NIP)) // field validation
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            if (string.IsNullOrEmpty(membershipID)) // field validation
                throw new ArgumentException($"El ID no puede estar vacío. {nameof(membershipID)}");
            if (!await ValidateMembershipID(membershipID)) // field validation
                throw new ArgumentException($"La membresía con ID: {membershipID} no existe.");
            try
            {
                // get client
                var client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .FirstOrDefaultAsync();

                // get membership
                var membership = await _membershipCollection
                    .Find(membership => membership.Membresia_ID == membershipID)
                    .FirstOrDefaultAsync();

                // assignament
                var assignament = new ClientsModel
                {
                    Membresia = membershipID,
                    Fecha_Inicio = DateOnly.FromDateTime(DateTime.Now),
                    Fecha_Fin = DateOnly.FromDateTime(DateTime.Now).AddDays(membership.Duracion),
                    Estado = true
                };

                // check if is not null
                if (assignament == null)
                    throw new ArgumentException("Asignación fallida.");

                // assign 
                var assign = Builders<ClientsModel>.Update
                    .Set(client => client.Membresia, membershipID)
                    .Set(client => client.Fecha_Inicio, DateOnly.FromDateTime(DateTime.Now))
                    .Set(client => client.Fecha_Fin, DateOnly.FromDateTime(DateTime.Now).AddDays(membership.Duracion))
                    .Set(client => client.Estado, true);

                // update
                await _clientsCollection
                    .UpdateOneAsync(client => client.NIP == NIP, assign);

                // client information
                var clientWithMembership = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .FirstOrDefaultAsync();

                return clientWithMembership;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al al asignar una membresía al cliente. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}

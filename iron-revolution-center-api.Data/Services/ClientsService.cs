using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using MongoDB.Bson;
using MongoDB.Driver;
using static MongoDB.Driver.WriteConcern;

namespace iron_revolution_center_api.Data.Service
{
    // client services
    public class ClientsService : iClientsService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;
        private readonly IMongoCollection<MembershipsModel> _membershipCollection;
        private readonly IMongoCollection<RegisterClientDTO> _registerClientCollection;
        private readonly IMongoCollection<ModifyClientDTO> _modifyClientCollection;
        private readonly IMongoCollection<MembershipsModel> _membersCollection;
        private readonly IMongoCollection<BranchesModel> _branchesCollection;

        // method to exclude _id field
        private static ProjectionDefinition<ClientsModel> ExcludeIdProjection()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }
        private static ProjectionDefinition<MembershipsModel> ExcludeIdProjectionMemberships()
        {
            return Builders<MembershipsModel>.Projection.Exclude("_id");
        }
        private static ProjectionDefinition<BranchesModel> ExcludeIdProjectionBranches()
        {
            return Builders<BranchesModel>.Projection.Exclude("_id");
        }

        public ClientsService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
            _membershipCollection = _mongoDatabase.GetCollection<MembershipsModel>("Memberships");
            _registerClientCollection = _mongoDatabase.GetCollection<RegisterClientDTO>("Clients");
            _modifyClientCollection = _mongoDatabase.GetCollection<ModifyClientDTO>("Clients");
            _membersCollection = _mongoDatabase.GetCollection<MembershipsModel>("Memberships");
            _branchesCollection = _mongoDatabase.GetCollection<BranchesModel>("Branches");
        }
        #endregion

        #region Validations
        private async Task<bool> IsClientRegistered(string NIP)
        {
            try
            {
                if (string.IsNullOrEmpty(NIP)) return true;

                // check client 
                var NIPIsAlreadyUsed = await _clientsCollection
                    .CountDocumentsAsync(client => client.NIP == NIP);

                // validate existence
                return NIPIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> IsSegurityKeyAlreadyUsed(string segurityKey)
        {
            try
            {
                // check segurity key
                var segurityKeyIsAlreadyUsed = await _clientsCollection
                    .CountDocumentsAsync(client => client.Clave_Seguridad == segurityKey);

                // validate existence
                return segurityKeyIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        private async Task<bool> IsPhoneAlreadyUsed(string phone)
        {
            try
            {
                // check client number
                var phoneIsAlreadyUsed = await _clientsCollection
                    .CountDocumentsAsync(client => client.Celular == phone);

                // validate existence
                return phoneIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> IsBranchIdAlreadyUsed(string branchId)
        {
            try
            {
                // check branch
                var branchExists = await _branchesCollection
                    .CountDocumentsAsync(branch => branch.Sucursal_Id == branchId);

                // validate existence
                return branchExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<byte[]> GetBytesFromFile(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        #endregion

        #region ListClients
        public async Task<IEnumerable<ClientsModel>> ListClients(string membershipId, DateOnly startDay, DateOnly endDay)
        {
            try
            {
                // constructors
                var filterBuilder = Builders<ClientsModel>.Filter;
                var filter = new List<FilterDefinition<ClientsModel>>();

                if (!string.IsNullOrEmpty(membershipId))
                {
                    var membership = await _membersCollection
                        .Find(membership => membership.Membresia_Id == membershipId)
                        .Project<MembershipsModel>(ExcludeIdProjectionMemberships())
                        .FirstOrDefaultAsync();

                    filter.Add(filterBuilder.Eq(client => client.Membresia, membership.Nombre));
                }

                // Gte: Greate That or Equal
                filter.Add(filterBuilder.Gte(client => client.Fecha_Inicio, startDay));
                // Lte: Less That or Equal
                filter.Add(filterBuilder.Lte(client => client.Fecha_Fin, endDay));

                var dateFilter = filterBuilder.Or(
                    filterBuilder.And(
                        filterBuilder.Gte(client => client.Fecha_Inicio, startDay),
                        filterBuilder.Lte(client => client.Fecha_Inicio, endDay)
                    ),
                    filterBuilder.And(
                        filterBuilder.Gte(client => client.Fecha_Fin, startDay),
                        filterBuilder.Lte(client => client.Fecha_Fin, endDay)
                    )
                );

                filter.Add(dateFilter);

                var filters = filter.Any() ? filterBuilder.And(filter) : filterBuilder.Empty;

                // get clients
                return await _clientsCollection
                    .Find(filters)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar clientes. {ex}");
            }
        }
        #endregion

        #region GetClientByNIP
        public async Task<ClientsModel> GetClientByNIP(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                throw new ArgumentException($"El NIP no puede estar vacío.");
            if (!await IsClientRegistered(NIP))
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // get client
                return await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar cliente. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterClient
        public async Task<ClientsModel> RegisterClient(newClientModel clientDTO)
        {
            if (string.IsNullOrEmpty(clientDTO.Celular)) 
                throw new ArgumentException($"El número de celular no puede estar vacío.");
            if (await IsPhoneAlreadyUsed(clientDTO.Celular)) 
                throw new ArgumentException($"Número de celular: {clientDTO.Celular} ya en uso");
            if (string.IsNullOrEmpty(clientDTO.sucursal_Id))
                throw new ArgumentException($"El ID de la sucursal no puede estar vacío.");
            if (!await IsBranchIdAlreadyUsed(clientDTO.sucursal_Id))
                throw new ArgumentException($"El ID: {clientDTO.sucursal_Id} no existe.");
            try
            {
                byte[]? fotoBytes = null;
                if (!string.IsNullOrEmpty(clientDTO.Foto))
                    fotoBytes = Convert.FromBase64String(clientDTO.Foto);

                // generate a unique nip
                string NIP;
                do
                    NIP = new Random().Next(1, 10000).ToString("D6");
                while (await IsClientRegistered(NIP));

                // generate a unique seguirity key
                string SegurityKey;
                do
                    SegurityKey = new Random().Next(1, 100000).ToString("D5");
                while (await IsSegurityKeyAlreadyUsed(SegurityKey));

                var branch = await _branchesCollection
                    .Find(branch => branch.Sucursal_Id == clientDTO.sucursal_Id)
                    .Project<BranchesModel>(ExcludeIdProjectionBranches())
                    .FirstOrDefaultAsync();

                var newClient = new RegisterClientDTO
                {
                    NIP = NIP,
                    Foto = fotoBytes,
                    Clave_Seguridad = SegurityKey,
                    Nombre_Completo = clientDTO.Nombre_Completo,
                    Celular = clientDTO.Celular,
                    Observacion = clientDTO.Observacion,
                    Sucursal = new BranchesModel
                    {
                        Sucursal_Id = branch.Sucursal_Id,
                        Nombre = branch.Nombre,
                        Ubicacion = branch.Ubicacion
                    }
                };

                // check if is not null
                if (newClient == null)
                    throw new ArgumentException($"Registro fallido.");

                // insert
                await _registerClientCollection.InsertOneAsync(newClient);

                return await GetClientByNIP(newClient.NIP);
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar cliente. {ex}");
            } catch (ArgumentException ex) {
                // in case of error 
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyClient
        public async Task<ClientsModel> ModifyClient(string NIP, modifyClientModel clientDTO)
        {
            if (string.IsNullOrEmpty(NIP)) 
                throw new ArgumentException($"El NIP no puede estar vacío.");
            if (!await IsClientRegistered(NIP)) 
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // create update definitions 
                var updateBuilder = Builders<ClientsModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<ClientsModel>>();

                // modify not null field
                if (clientDTO.Foto != null) // photo
                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Foto, clientDTO.Foto));
                if (!string.IsNullOrEmpty(clientDTO.Nombre_Completo)) // full name
                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Nombre_Completo, clientDTO.Nombre_Completo));
                if (!string.IsNullOrEmpty(clientDTO.Celular)) // phone
                    updateDefinitions.Add(updateBuilder
                                         .Set(client => client.Celular, clientDTO.Celular));
                if (!string.IsNullOrEmpty(clientDTO.Observacion)) // observation
                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Observacion, clientDTO.Observacion));
                if(!string.IsNullOrEmpty(clientDTO.sucursal_Id)) // branch
                {
                    var branch = await _branchesCollection
                    .Find(branch => branch.Sucursal_Id == clientDTO.sucursal_Id)
                    .Project<BranchesModel>(ExcludeIdProjectionBranches())
                    .FirstOrDefaultAsync();

                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Sucursal.Sucursal_Id, clientDTO.sucursal_Id)
                                     .Set(client => client.Sucursal.Nombre, branch.Nombre)
                                     .Set(client => client.Sucursal.Ubicacion, branch.Ubicacion));
                }
                // verification
                if (!updateDefinitions.Any())
                    throw new ArgumentException("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<ClientsModel>
                    .Filter
                    .Eq(client => client.NIP, NIP);

                // update
                var update = await _clientsCollection
                    .UpdateOneAsync(filter, combine);

                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar cliente.");

                return await GetClientByNIP(NIP);
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al modificar cliente con el NIP: {NIP}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteClient
        public async Task<ClientsModel> DeleteClient(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                throw new ArgumentException($"El NIP no puede estar vacío.");
            if (!await IsClientRegistered(NIP))
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // client
                var client = await GetClientByNIP(NIP);

                // delete
                var delete = await _clientsCollection
                    .DeleteOneAsync(client => client.NIP == NIP);

                // check
                if (delete.DeletedCount == 0)
                    throw new ArgumentException("Error al eliminar cliente.");

                return client;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al eliminar al cliente con el NIP: {NIP}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region GenerateSegurityKey
        public async Task<ClientsModel> GenerateSegurityKey(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                throw new ArgumentException($"El NIP no puede estar vacío.");
            if (!await IsClientRegistered(NIP))
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // create update definitions
                var updateBuilder = Builders<ClientsModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<ClientsModel>>();

                // generate a unique seguirity key
                string SegurityKey;
                do
                    SegurityKey = new Random().Next(1, 100000).ToString("D5");
                while (await IsSegurityKeyAlreadyUsed(SegurityKey));

                if (!string.IsNullOrEmpty(NIP)) // segurity key
                    updateDefinitions.Add(updateBuilder
                    .Set(client => client.Clave_Seguridad, SegurityKey));

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<ClientsModel>
                    .Filter
                    .Eq(client => client.NIP, NIP);
                var update = await _clientsCollection
                    .UpdateOneAsync(filter, combine);

                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al generar clave de seguriddad.");

                // client
                return await GetClientByNIP(NIP);
            } catch (MongoException ex) {
                // in case of erro
                throw new InvalidOperationException($"Error al generar clave de seguridad. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}

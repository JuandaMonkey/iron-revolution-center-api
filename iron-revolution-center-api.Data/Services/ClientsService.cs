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
        private readonly IMongoCollection<RegisterClientDTO> _registerClientCollection;
        private readonly IMongoCollection<ModifyClientDTO> _modifyClientCollection;
        private readonly IMongoCollection<MembershipsModel> _membersCollection;

        // method to exclude _id field
        private static ProjectionDefinition<ClientsModel> ExcludeIdProjection()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }

        public ClientsService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
            _registerClientCollection = _mongoDatabase.GetCollection<RegisterClientDTO>("Clients");
            _modifyClientCollection = _mongoDatabase.GetCollection<ModifyClientDTO>("Clients");
            _membersCollection = _mongoDatabase.GetCollection<MembershipsModel>("Memberships");
        }
        #endregion

        #region Validations
        private async Task<bool> IsClientRegistered(string NIP)
        {
            try
            {
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
        #endregion

        #region ListClients
        public async Task<IEnumerable<ClientsModel>> ListClients()
        {
            try
            {
                // get clients
                return await _clientsCollection
                    .Find(FilterDefinition<ClientsModel>.Empty)
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
        public async Task<ClientsModel> RegisterClient(RegisterClientDTO clientDTO)
        {
            if (string.IsNullOrEmpty(clientDTO.Celular)) 
                throw new ArgumentException($"El número de celular no puede estar vacío.");
            if (await IsPhoneAlreadyUsed(clientDTO.Celular)) 
                throw new ArgumentException($"Número de celular: {clientDTO.Celular} ya en uso");
            try
            {
                // generate a unique nip
                string NIP;
                do
                    NIP = new Random().Next(1, 10000).ToString("D5");
                while (await IsClientRegistered(NIP));

                clientDTO.NIP = NIP;

                // generate a unique seguirity key
                string SegurityKey;
                do
                    SegurityKey = new Random().Next(1, 100000).ToString("D5");
                while (await IsSegurityKeyAlreadyUsed(SegurityKey));

                clientDTO.Clave_Seguridad = SegurityKey;

                // register client
                var newClient = new RegisterClientDTO
                {
                    NIP = clientDTO.NIP,
                    Foto = clientDTO.Foto,
                    Clave_Seguridad = clientDTO.Clave_Seguridad,
                    Nombre_Completo = clientDTO.Nombre_Completo,
                    Celular = clientDTO.Celular,
                    Observacion = clientDTO.Observacion
                };

                // check if is not null
                if (newClient == null)
                    throw new ArgumentException($"Registro fallido.");

                // register
                await _registerClientCollection.InsertOneAsync(newClient);

                // client
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
        public async Task<ClientsModel> ModifyClient(string NIP, ModifyClientDTO clientDTO)
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
                {
                    if (await IsPhoneAlreadyUsed(clientDTO.Celular))
                        throw new ArgumentException($"Número de celular: {clientDTO.Celular} ya en uso");
                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Celular, clientDTO.Celular));
                }
                if (!string.IsNullOrEmpty(clientDTO.Observacion)) // observation
                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Observacion, clientDTO.Observacion));

                // verification
                if (!updateDefinitions.Any())
                    throw new ArgumentException("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<ClientsModel>
                    .Filter
                    .Eq(client => client.NIP, NIP);
                // result of the modification
                var update = await _clientsCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar cliente.");

                // client
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
                var client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // execute query
                var delete = await _clientsCollection
                    .DeleteOneAsync(client => client.NIP == NIP);

                // check
                if (delete.DeletedCount == 0)
                    throw new ArgumentException("Error al eliminar cliente.");

                // return of result of elimination
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

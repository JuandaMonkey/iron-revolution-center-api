using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private async Task<bool> ValidateClientNIP(string NIP)
        {
            try
            {
                // check client NIP
                var NIPIsAlreadyUsed = await _clientsCollection
                    .CountDocumentsAsync(client => client.NIP == NIP);

                // validate existence
                return NIPIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> ValidateClientPhone(string phone)
        {
            try
            {
                // check client number
                var phoneIsAlreadyUsed = await _clientsCollection
                    .CountDocumentsAsync(client => client.Celular == phone);

                // validate existence
                return phoneIsAlreadyUsed > 0;
            }
            catch {
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
                // get the collection clients
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
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException($"El NIP no puede estar vacío. {nameof(NIP)}");
            if (!await ValidateClientNIP(NIP)) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // get client
                var client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();
                // get membership
                var membership = await _membersCollection
                    .Find(membership => membership.Membresia_ID == client.Membresia)
                    .FirstOrDefaultAsync();

                // client
                var clientInformation = new ClientsModel
                {
                    NIP = client.NIP,
                    Foto = client.Foto,
                    Nombre_Completo = client.Nombre_Completo,
                    Celular = client.Celular,
                    Observacion = client.Observacion,
                    Membresia = membership.Nombre,
                    Fecha_Inicio = client.Fecha_Inicio,
                    Fecha_Fin = client.Fecha_Fin,
                    Estado = client.Estado

                };

                return clientInformation;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al al mostrar cliente. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterClient
        public async Task<RegisterClientDTO> RegisterClient(RegisterClientDTO clientDTO)
        {
            if (string.IsNullOrEmpty(clientDTO.Nombre_Completo)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(clientDTO.Nombre_Completo)}");
            if (string.IsNullOrEmpty(clientDTO.Celular)) // field verification
                throw new ArgumentException($"El número de celular no puede estar vacío. {nameof(clientDTO.Celular)}");
            if (await ValidateClientPhone(clientDTO.Celular) == true) // fiel verification
                throw new ArgumentException($"Número de celular: {clientDTO.Celular} ya en uso");
            try
            {
                // generate a unique nip
                string NIP;
                do
                    NIP = new Random().Next(1, 10000).ToString("D5");
                // check if nip is already used
                while (await ValidateClientNIP(NIP) == true);

                // register client
                var newClient = new RegisterClientDTO
                {
                    NIP = NIP,
                    Foto = clientDTO.Foto,
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
                return newClient;
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
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP no puede estar vacío.", nameof(NIP));
            if (!await ValidateClientNIP(NIP)) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // create update definitions 
                var updateBuilder = Builders<ClientsModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<ClientsModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(clientDTO.Foto)) // photo
                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Foto, clientDTO.Foto));
                if (!string.IsNullOrEmpty(clientDTO.Nombre_Completo)) // full name
                    updateDefinitions.Add(updateBuilder
                                     .Set(client => client.Nombre_Completo, clientDTO.Nombre_Completo));
                if (!string.IsNullOrEmpty(clientDTO.Celular)) // phone
                {
                    if (await ValidateClientPhone(clientDTO.Celular))
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

                // find client
                ClientsModel client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // client
                return client;
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
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException($"El NIP no puede estar vacío. {nameof(NIP)}");
            if (!await ValidateClientNIP(NIP)) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // client
                ClientsModel client = await _clientsCollection
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
    }
}

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
                    .CountDocumentsAsync(client => client.Phone == phone);

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
        public async Task<IEnumerable<ClientsModel>> GetClientByNIP(string NIP)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP no puede estar vacío.", nameof(NIP));
            if (await ValidateClientNIP(NIP) == false) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // get clients
                return await _clientsCollection
                    .Find(Client => Client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .ToListAsync();
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
            if (string.IsNullOrEmpty(clientDTO.Full_Name)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(clientDTO.Full_Name)}");
            if (string.IsNullOrEmpty(clientDTO.Phone)) // field verification
                throw new ArgumentException($"El número de celular no puede estar vacío. {nameof(clientDTO.Phone)}");
            if (await ValidateClientPhone(clientDTO.Phone) == true) // fiel verification
                throw new ArgumentException($"Número de celular: {clientDTO.Phone} ya en uso");
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
                    Photo = clientDTO.Photo,
                    Full_Name = clientDTO.Full_Name,
                    Phone = clientDTO.Phone,
                    Observation = clientDTO.Observation
                };

                // check if is not null
                if (newClient == null)
                    throw new Exception($"Registro fallido.");

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
            } catch (Exception ex) {
                // in case of error 
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyClient
        public async Task<ClientsModel> ModifyClient(string NIP, ModifyClientDTO clientDTO)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP no puede estar vacío.", nameof(NIP));
            if (await ValidateClientNIP(NIP) == false) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // create update definitions 
                var updateBuilder = Builders<ClientsModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<ClientsModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(clientDTO.Photo)) // photo
                    updateDefinitions.Add(updateBuilder.Set(client => client.Photo, clientDTO.Photo));
                if (!string.IsNullOrEmpty(clientDTO.Full_Name)) // full name
                    updateDefinitions.Add(updateBuilder.Set(client => client.Full_Name, clientDTO.Full_Name));
                if (!string.IsNullOrEmpty(clientDTO.Phone)) // phone
                {
                    if (await ValidateClientPhone(clientDTO.Phone))
                        throw new ArgumentException($"El número de teléfono: {clientDTO.Phone} ya está en uso.");
                    updateDefinitions.Add(updateBuilder.Set(client => client.Phone, clientDTO.Phone));
                }
                if (!string.IsNullOrEmpty(clientDTO.Observation)) // observation
                    updateDefinitions.Add(updateBuilder.Set(client => client.Observation, clientDTO.Observation));

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

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
                    throw new Exception("Error al modificar cliente.");

                // find client
                ClientsModel client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // client
                return client;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al modificar cliente con el NIP {NIP}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            } catch (Exception ex) {
                // in case of error
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteClient
        public async Task<ClientsModel> DeleteClient(string NIP)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP no puede estar vacío.", nameof(NIP));
            if (await ValidateClientNIP(NIP) == false) // field verification
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
                    throw new Exception("Error al eliminar cliente.");

                // return of result of elimination
                return client;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al eliminar al cliente con el NIP: {NIP}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            } catch (Exception ex) {
                // in case of error
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion
    }
}

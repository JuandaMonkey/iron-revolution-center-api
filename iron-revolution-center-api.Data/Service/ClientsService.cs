using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Clients;
using iron_revolution_center_api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using static MongoDB.Driver.WriteConcern;

namespace iron_revolution_center_api.Data.Service
{
    // client services
    public class ClientsService : iClientsService
    {
        #region mongodb
        private readonly IMongoDatabase _mongoDatabase;

        public ClientsService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        // method to exclude _id field
        private static ProjectionDefinition<ClientsModel> ExcludeIdProjection()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }
        #endregion

        #region ListClients
        public async Task<IEnumerable<ClientsModel>> ListClients()
        {
            try
            {
                // get the collection
                var collection = _mongoDatabase.GetCollection<ClientsModel>("Clients");

                // execute the query
                return await collection
                    .Find(FilterDefinition<ClientsModel>.Empty)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException("Error al mostrar clientes.", ex);
            }
        }
        #endregion

        #region GetClientByNIP
        public async Task<IEnumerable<ClientsModel>> GetClientByNIP(string NIP)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP no puede estar vacío.", nameof(NIP));

            try
            {
                // get the collection
                var collection = _mongoDatabase.GetCollection<ClientsModel>("Clients");

                // execute the query
                return await collection
                    .Find(Client => Client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar cliente con el NIP: {NIP}.", ex);
            }
        }
        #endregion

        #region RegisterClient
        public async Task<RegisterClientsDTos> RegisterClient(RegisterClientsDTos clientDTO)
        {
            if (clientDTO == null) // field verification
                throw new ArgumentNullException(nameof(clientDTO));

            try
            {
                // get the collection
                var collection = _mongoDatabase.GetCollection<RegisterClientsDTos>("Clients");

                // generate a unique nip
                string NIP;
                do
                {
                    NIP = new Random().Next(1, 10000).ToString("D5");
                } 
                // as long as it exists in the database
                while (await collection.Find(ClientNIP => ClientNIP.NIP == NIP).AnyAsync());

                // check if the phone is already used
                if (await collection.Find(Phone => Phone.Phone == clientDTO.Phone).AnyAsync())
                {
                    throw new InvalidOperationException($"Número de celular {clientDTO.Phone} ya en uso");
                }

                // asign NIP
                clientDTO.NIP = NIP;

                // insert client
                await collection.InsertOneAsync(clientDTO);

                // return a client
                return clientDTO;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException("Error al registrar cliente.", ex);
            }
        
        }
        #endregion

        #region ModifyClient
        public async Task<ClientsModel> ModifyClient(string NIP, ModifyClientDTOs clientDTO)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP no puede estar vacío.", nameof(NIP));
            if (clientDTO == null) // field verification
                throw new ArgumentNullException(nameof(clientDTO));

            try
            {
                // get the collection
                var collection = _mongoDatabase.GetCollection<ClientsModel>("Clients");


                // filter to find by NIP
                var filter = Builders<ClientsModel>.Filter.Eq(client => client.NIP, NIP);

                // create update definitions 
                var updateBuilder = Builders<ClientsModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<ClientsModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(clientDTO.Photo)) // photo
                    updateDefinitions.Add(updateBuilder.Set(c => c.Photo, clientDTO.Photo));
                if (!string.IsNullOrEmpty(clientDTO.Full_Name)) // full name
                    updateDefinitions.Add(updateBuilder.Set(c => c.Full_Name, clientDTO.Full_Name));
                if (!string.IsNullOrEmpty(clientDTO.Phone)) // phone
                    updateDefinitions.Add(updateBuilder.Set(c => c.Phone, clientDTO.Phone));
                if (!string.IsNullOrEmpty(clientDTO.Observation)) // observation
                    updateDefinitions.Add(updateBuilder.Set(c => c.Observation, clientDTO.Observation));

                // verification
                if (!updateDefinitions.Any())
                    throw new ArgumentException("No se proporcionaron campos válidos para modificar");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);
                // result of the modification
                var update = await collection.UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new InvalidOperationException("Error al modificar cliente");

                // client
                ClientsModel client = await collection.Find(filter)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // return of result of modify
                return client;

            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al modificar cliente con el NIP {NIP}.", ex);
            }
        }
        #endregion

        #region delete_client
        public async Task<ClientsModel> DeleteClient(string NIP)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException("El NIP no puede estar vacío.", nameof(NIP));

            try
            {
                // get the collection
                var collection = _mongoDatabase.GetCollection<ClientsModel>("Clients");

                // filter to find by NIP
                var filter = Builders<ClientsModel>.Filter.Eq(client => client.NIP, NIP);

                // client
                ClientsModel client = await collection.Find(filter)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // execute query
                var delete = await collection.DeleteOneAsync(filter);
                 
                // check
                if (delete.DeletedCount == 0)
                    throw new InvalidOperationException("Error al eliminar cliente");

                // return of result of elimination
                return client;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error deleting client with NIP {NIP}.", ex);
            }
        }
        #endregion
    }
}

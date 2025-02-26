using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Activity_Center;
using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace iron_revolution_center_api.Data.Service
{
    public class Activity_CenterService : iActivity_CenterService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<Activity_CenterModel> _activityCenterCollection;
        private readonly IMongoCollection<EntryClientDTO> _EntryClientCollection;
        private readonly IMongoCollection<ExitClientDTO> _ExitClientCollection;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;
        private readonly IMongoCollection<Branches_OfficeModel> _branchesOfficeCollection;

        // method to exclude _id field
        private static ProjectionDefinition<Activity_CenterModel> ExcludeIdProjection()
        {
            return Builders<Activity_CenterModel>.Projection.Exclude("_id");
        }

        public Activity_CenterService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _activityCenterCollection = _mongoDatabase.GetCollection<Activity_CenterModel>("Activity_Center");
            _EntryClientCollection = _mongoDatabase.GetCollection<EntryClientDTO>("Activity_Center");
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
            _branchesOfficeCollection = _mongoDatabase.GetCollection<Branches_OfficeModel>("Branches_Office");
        }
        #endregion

        #region Validations
        private async Task<bool> IsClient(string NIP)
        {
            try
            {
                // get client
                var client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .FirstOrDefaultAsync();

                // check 
                if (client == null)
                    throw new ArgumentException("Cliente no encontrado.");

                // check 
                if (client.Estado == false)
                    throw new ArgumentException("Estado finalizado.");

                // check 
                var DateNow = DateOnly.FromDateTime(DateTime.Now);
                if (client.Fecha_Fin < DateNow)
                    throw new ArgumentException("Membresía finalizada.");

                return true;
            } catch {
                // in case of error
                return false;
            }
        }

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

        private async Task<bool> ValidateBranchOfficeID(string branchID)
        {
            try
            {
                // check branch office exists
                var branchOfficeExists = await _branchesOfficeCollection
                    .CountDocumentsAsync(branchOffice => branchOffice.Branche_ID == branchID);

                // validate existence
                return branchOfficeExists > 0;
            } catch {
                // if not in used
                return false;
            }
        }
        #endregion

        #region ListActivity
        public async Task<IEnumerable<Activity_CenterModel>> ListActivity()
        {
            try
            {
                //
                var clients = await _clientsCollection
                    .Find(_ => true)
                    .ToListAsync();

                //
                var branches = await _branchesOfficeCollection
                    .Find(_ => true)
                    .ToListAsync();

                // 
                var activities = await _activityCenterCollection
                    .Find(_ => true)
                    .ToListAsync();

                //
                var activityCenter = activities.Select(activity => new Activity_CenterModel
                {
                    Cliente = clients.FirstOrDefault(client => client.NIP == activity.Cliente), 
                    Entrada = activity.Entrada,
                    Salida = activity.Salida,
                    Sucursal = branches.FirstOrDefault(branchOffice => branchOffice.Branche_ID == activity.Sucursal) 
                }).ToList();

                //
                return activityCenter;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar actividad. {ex}");
            }
        }
        #endregion

        #region RegisterEntry
        public async Task<bool> RegisterEntry(string NIP, string branchOffice)
        {
            if (string.IsNullOrEmpty(NIP)) // field verification
                throw new ArgumentException($"El NIP no puede estar vacío. {nameof(NIP)}");
            if (!await ValidateClientNIP(NIP)) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            if (string.IsNullOrEmpty(branchOffice)) // field verification
                throw new ArgumentException($"La sucursal no puede estar vacío. {nameof(branchOffice)}");
            if (!await ValidateBranchOfficeID(branchOffice)) // field verification
                throw new ArgumentException($"La sucursal con el ID: {nameof(branchOffice)} no existe.");
            try
            {
                // verifica si el cliente es apto para su ingreso
                bool client = await IsClient(NIP);
                if (!client)
                    throw new ArgumentException("No apto para entrar.");

                // client entry
                var clientEnty = new EntryClientDTO
                {
                    Cliente = NIP,
                    Entrada = DateTime.UtcNow,
                    Salida = DateTime.UtcNow.AddHours(24),
                    Sucursal = branchOffice
                };

                //
                if (clientEnty == null)
                    throw new ArgumentException("Registro fallido.");

                // register
                await _EntryClientCollection.InsertOneAsync(clientEnty);

                return true;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar entrada. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterExit
        public async Task<bool> RegisterExit(string NIP)
        {
            if (string.IsNullOrEmpty(NIP)) //
                throw new ArgumentException($"El NIP no puede estar vacío. {nameof(NIP)}");
            if (!await ValidateClientNIP(NIP)) // field verification
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                //
                var clientEntry = await _activityCenterCollection
                    .Find(client => client.NIP == NIP && client.date_exit > DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                //
                if (clientEntry == null)
                    throw new ArgumentException("Entrada inexistente."); //

                //
                var filter = Builders<Activity_CenterModel>
                                .Filter.Eq(assign => assign.NIP, NIP)
                             & Builders<Activity_CenterModel>
                                .Filter.Eq(assign => assign.date_entry, clientEntry.date_entry);

                //
                var update = Builders<Activity_CenterModel>.Update
                    .Set(assign => assign.date_exit, DateTime.UtcNow);

                //
                await _activityCenterCollection.UpdateOneAsync(filter, update);

                //
                return true;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar salida. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}

using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Activity_Center;
using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
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
        private readonly IMongoCollection<EntryClientDTO> _entryClientCollection;
        private readonly IMongoCollection<ExitClientDTO> _exitClientCollection;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;
        private readonly IMongoCollection<BranchesModel> _branchesCollection;

        // method to exclude _id field
        private static ProjectionDefinition<Activity_CenterModel> ExcludeIdProjection()
        {
            return Builders<Activity_CenterModel>.Projection.Exclude("_id");
        }

        private static ProjectionDefinition<ClientsModel> ExcludeIdProjectionClient()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }

        private static ProjectionDefinition<BranchesModel> ExcludeIdProjectionBranch()
        {
            return Builders<BranchesModel>.Projection.Exclude("_id");
        }

        public Activity_CenterService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _activityCenterCollection = _mongoDatabase.GetCollection<Activity_CenterModel>("Activity_Center");
            _entryClientCollection = _mongoDatabase.GetCollection<EntryClientDTO>("Activity_Center");
            _exitClientCollection = _mongoDatabase.GetCollection<ExitClientDTO>("Activity_Center");
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
            _branchesCollection = _mongoDatabase.GetCollection<BranchesModel>("Branches");
        }
        #endregion

        #region Validations
        private async Task<bool> IsClientRegistered(string NIP)
        {
            try
            {
                var clientCount = await _clientsCollection
                    .CountDocumentsAsync(client => client.NIP == NIP);
                return clientCount > 0;
            } catch {
                return false;
            }
        }

        private async Task<bool> IsBranchValid(string branchId)
        {
            try
            {
                var branchCount = await _branchesCollection
                    .CountDocumentsAsync(branchOffice => branchOffice.Sucursal_Id == branchId);
                return branchCount > 0;
            } catch {
                return false;
            }
        }

        private async Task<bool> HasActiveMembership(string NIP)
        {
            try
            {
                var client = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjectionClient())
                    .FirstOrDefaultAsync();

                if (client == null)
                    throw new ArgumentException("Cliente no encontrado.");

                if (string.IsNullOrEmpty(client.Membresia)) 
                    throw new ArgumentException("El cliente no tiene membresía asignada.");

                if (client.Fecha_Inicio == null || client.Fecha_Fin == null)
                    throw new ArgumentException("Membresía sin fechas válidas.");

                var dateNow = DateOnly.FromDateTime(DateTime.Now);
                if (client.Fecha_Fin < dateNow)
                    throw new ArgumentException("Membresía finalizada.");

                return true;
            } catch (Exception ex) {
                return false;
                throw new InvalidOperationException($"Error en HasActiveMembership: {ex.Message}");
            }
        }

        private async Task<bool> HasExistingEntryToday(string NIP)
        {
            try
            {
                var startDay = DateTime.UtcNow.Date;
                var endDay = startDay.AddDays(1).AddTicks(-1);

                var existingEntryCount = await _activityCenterCollection
                    .CountDocumentsAsync(client => client.Cliente.NIP == NIP &&
                                                client.Entrada >= startDay &&
                                                client.Entrada <= endDay &&
                                                client.Salida == null);

                return existingEntryCount > 0;
            } catch (Exception ex) {
                return false;
                throw new InvalidOperationException($"Error en HasExistingEntryToday: {ex.Message}");
            }
        }
        #endregion

        #region ListActivity
        public async Task<IEnumerable<Activity_CenterModel>> ListActivityCenter(string branchId, DateTime startDay, DateTime endDay)
        {
            try
            {
                var filterBuilder = Builders<Activity_CenterModel>.Filter;
                var filter = new List<FilterDefinition<Activity_CenterModel>>();

                if (!string.IsNullOrEmpty(branchId))
                    filter.Add(filterBuilder.Eq(activity_center => activity_center.Sucursal.Sucursal_Id, branchId));

                // Gte: Greate That or Equal 
                filter.Add(filterBuilder.Gte(activity_center => activity_center.Entrada, startDay));
                // Lte: Less That or Equal
                filter.Add(filterBuilder.Lte(activity_center => activity_center.Salida, endDay));

                var dateFilter = filterBuilder.Or(
                    filterBuilder.And(
                        filterBuilder.Gte(activity_center => activity_center.Entrada, startDay),
                        filterBuilder.Lte(activity_center => activity_center.Entrada, endDay)
                    ),
                    filterBuilder.And(
                        filterBuilder.Gte(activity_center => activity_center.Salida, startDay),
                        filterBuilder.Lte(activity_center => activity_center.Salida, endDay)
                    )
                );

                filter.Add(dateFilter);

                var filters = filter.Any() ? filterBuilder.And(filter) : filterBuilder.Empty;

                return await _activityCenterCollection
                    .Find(filters)
                    .Project<Activity_CenterModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al mostrar actividad. {ex}");
            }
        }
        #endregion

        #region RegisterEntry
        public async Task<bool> RegisterEntry(string NIP, string securityKey, string branchId)
        {
            if (string.IsNullOrWhiteSpace(NIP))
                throw new ArgumentException("El NIP no puede estar vacío.");
            if (!await IsClientRegistered(NIP))
                throw new ArgumentException($"NIP {NIP} no existe.");
            if (string.IsNullOrWhiteSpace(securityKey))
                throw new ArgumentException("La clave de seguridad no puede estar vacío.");
            var clientSKey = await _clientsCollection
                .Find(c => c.NIP == NIP)
                .Project<ClientsModel>(ExcludeIdProjectionClient())
                .FirstOrDefaultAsync();
            if (clientSKey.Clave_Seguridad != securityKey)
                throw new ArgumentException("Clave de seguridad incorrecta.");
            if (string.IsNullOrWhiteSpace(branchId))
                throw new ArgumentException("Sucursal no puede estar vacío.");
            if (!await IsBranchValid(branchId))
                throw new ArgumentException($"Sucursal con el ID {branchId} no existe.");

            try
            {
                if (await HasExistingEntryToday(NIP))
                    throw new ArgumentException("El cliente ya tiene un registro de entrada hoy.");

                if (!await HasActiveMembership(NIP))
                    throw new ArgumentException("No apto para entrar.");

                var branch = await _branchesCollection
                    .Find(branch => branch.Sucursal_Id == branchId)
                    .Project<BranchesModel>(ExcludeIdProjectionBranch())
                    .FirstOrDefaultAsync();

                if (branch == null)
                    throw new ArgumentException("Sucursal no encontrada.");

                var clientEntry = new EntryClientDTO
                {
                    Cliente = clientSKey,
                    Entrada = DateTime.UtcNow,
                    Salida = null, 
                    Sucursal = branch
                };

                if (clientEntry == null)
                    throw new ArgumentException("Registro fallido.");

                await _entryClientCollection.InsertOneAsync(clientEntry);

                return true;
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al registrar entrada. {ex}");
            } catch (ArgumentException ex) {
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterExit
        public async Task<bool> RegisterExit(string NIP)
        {
            if (string.IsNullOrWhiteSpace(NIP))
                throw new ArgumentException("NIP no puede estar vacío.");
            if (!await IsClientRegistered(NIP))
                throw new ArgumentException($"NIP {NIP} no existe.");
            try
            {
                var startDay = DateTime.UtcNow.Date;
                var endDay = startDay.AddDays(1).AddTicks(-1);

                var clientEntry = await _activityCenterCollection
                    .Find(client => client.Cliente.NIP == NIP &&
                                  client.Entrada >= startDay &&
                                  client.Entrada <= endDay &&
                                  client.Salida == null)
                    .SortByDescending(client => client.Entrada)
                    .Project<Activity_CenterModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                if (clientEntry == null)
                    throw new ArgumentException("No hay registro de entrada previo para este cliente hoy.");

                var filter = Builders<Activity_CenterModel>
                                .Filter.Eq(client => client.Cliente.NIP, NIP)
                             & Builders<Activity_CenterModel>
                                .Filter.Eq(client => client.Entrada, clientEntry.Entrada);

                var update = Builders<Activity_CenterModel>.Update
                    .Set(client => client.Salida, DateTime.UtcNow);

                await _activityCenterCollection.UpdateOneAsync(filter, update);

                return true;
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al registrar salida. {ex}");
            } catch (ArgumentException ex) {
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}
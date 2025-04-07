using iron_revolution_center_api.Data.Interfaces;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.DTOs.Statistics;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace iron_revolution_center_api.Data.Services
{
    public class StatisticsService : iStatisticsService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;
        private readonly IMongoCollection<BranchesModel> _branchesCollection;
        private readonly IMongoCollection<Activity_CenterModel> _activityCenterCollection;
        private readonly IMongoCollection<EmployeesModel> _employeesCollection;
        private readonly IMongoCollection<MembershipsModel> _membershipCollection;

        private static ProjectionDefinition<BranchesModel> ExcludeIdProjectionBranches()
        {
            return Builders<BranchesModel>.Projection.Exclude("_id");
        }
        private static ProjectionDefinition<MembershipsModel> ExcludeIdProjectionMemberships()
        {
            return Builders<MembershipsModel>.Projection.Exclude("_id");
        }
        private static ProjectionDefinition<Activity_CenterModel> ExcludeIdProjectionActivityCenter()
        {
            return Builders<Activity_CenterModel>.Projection.Exclude("_id");
        }

        public StatisticsService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
            _branchesCollection = _mongoDatabase.GetCollection<BranchesModel>("Branches");
            _activityCenterCollection = _mongoDatabase.GetCollection<Activity_CenterModel>("Activity_Center");
            _employeesCollection = _mongoDatabase.GetCollection<EmployeesModel>("Employees");
            _membershipCollection = _mongoDatabase.GetCollection<MembershipsModel>("Memberships");
        }
        #endregion

        #region Validations
        private async Task<bool> IsBranchIdAlreadyExists(string branchId)
        {
            try
            {
                var branchExists = await _branchesCollection
                    .CountDocumentsAsync(branch => branch.Sucursal_Id == branchId);

                return branchExists > 0;
            } catch {
                return false;
            }
        }
        #endregion

        public async Task<registeredClients> getRegisteredClients(string branchId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                if (!await IsBranchIdAlreadyExists(branchId))
                    throw new ArgumentException($"La sucursal: {branchId} no existe.");
            }
            try
            {
                var filterBuilder = Builders<ClientsModel>.Filter;
                var filter = new List<FilterDefinition<ClientsModel>>();

                if (!string.IsNullOrEmpty(branchId))
                    filter.Add(filterBuilder.Eq(clients => clients.Sucursal.Sucursal_Id, branchId));

                var filters = filter.Any() ? filterBuilder.And(filter) : filterBuilder.Empty;

                var registered = await _clientsCollection
                    .CountDocumentsAsync(filters);

                return new registeredClients
                {
                    Registered_Clients = registered
                };
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al contar clientes. {ex}");
            } catch (ArgumentException ex) {
                throw new ArgumentException($"Error: {ex}");
            }
        }

        public async Task<activeClients> getActiveClients(string branchId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                if (!await IsBranchIdAlreadyExists(branchId))
                    throw new ArgumentException($"La sucursal: {branchId} no existe.");
            }
            try
            {
                var filterBuilder = Builders<Activity_CenterModel>.Filter;
                var filter = filterBuilder.Empty;

                if (!string.IsNullOrEmpty(branchId))
                    filter &= filterBuilder.Eq(activity_center => activity_center.Sucursal.Sucursal_Id, branchId);

                filter &= filterBuilder.Lte(activity_center => activity_center.Entrada, DateTime.UtcNow) &
                 (filterBuilder.Eq(activity_center => activity_center.Salida, null));

                var activeCount = await _activityCenterCollection.CountDocumentsAsync(filter);

                return new activeClients
                {
                    Active_Clients = activeCount
                };
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al contar clientes. {ex}");
            } catch (ArgumentException ex) {
                throw new ArgumentException($"Error: {ex}");
            }
        }

        public async Task<registeredEmployees> getRegisteredEmployees(string branchId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                if (!await IsBranchIdAlreadyExists(branchId))
                    throw new ArgumentException($"La sucursal: {branchId} no existe.");
            }
            try
            {
                var filterBuilder = Builders<EmployeesModel>.Filter;
                var filter = new List<FilterDefinition<EmployeesModel>>();

                if (!string.IsNullOrEmpty(branchId))
                    filter.Add(filterBuilder.Eq(employee => employee.Sucursal.Sucursal_Id, branchId));

                var filters = filter.Any() ? filterBuilder.And(filter) : filterBuilder.Empty;

                var registered = await _employeesCollection
                    .CountDocumentsAsync(filters);

                return new registeredEmployees
                {
                    Registered_Employees = registered
                };
            }
            catch (MongoException ex)
            {
                throw new InvalidOperationException($"Error al contar clientes. {ex}");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Error: {ex}");
            }
        }

        public async Task<IEnumerable<branchesCount>> getBranchesCount()
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-31);
                var endDate = DateTime.UtcNow;

                var branches = await _branchesCollection
                    .Find(_ => true)
                    .Project<BranchesModel>(ExcludeIdProjectionBranches())
                    .ToListAsync();

                var aggregationResult = await _activityCenterCollection.Aggregate()
                    .Match(activity => activity.Entrada >= startDate && activity.Entrada <= endDate)
                    .Group(
                        key => key.Sucursal.Ubicacion,
                        group => new branchesCount
                        {
                            ubicacion = group.Key,
                            conteo = group.Count()
                        })
                    .ToListAsync();

                var result = branches.Select(branch => new branchesCount
                {
                    ubicacion = branch.Ubicacion,
                    conteo = aggregationResult.FirstOrDefault(a => a.ubicacion == branch.Ubicacion)?.conteo ?? 0
                })
                .OrderByDescending(order => order.conteo)
                .ToList();

                return result;
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al contar clientes por sucursal. {ex}");
            }
        }

        public async Task<branchesCount> getMostFrecuentedBranch(string branchId)
        {
            try
            {
                var branch = await getBranchesCount();

                if (!string.IsNullOrEmpty(branchId))
                {
                    if (!await IsBranchIdAlreadyExists(branchId))
                        throw new ArgumentException($"La sucursal: {branchId} no existe.");

                    var branchLocation = await _branchesCollection
                        .Find(branch => branch.Sucursal_Id == branchId)
                        .Project<BranchesModel>(ExcludeIdProjectionBranches())
                        .FirstOrDefaultAsync();

                    var specificBranch = branch.FirstOrDefault(branch => branch.ubicacion == branchLocation.Ubicacion);

                    return specificBranch;
                }

                var mostFrequented = branch.OrderByDescending(branch => branch.conteo).First();

                return mostFrequented;
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al contar clientes por sucursal. {ex}");
            }
        }

        private async Task<IEnumerable<membershipsCount>> getMembershipsCount()
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-31);
                var endDate = DateTime.UtcNow;

                var memberships = await _activityCenterCollection.Aggregate()
                        .Match(activity => activity.Entrada >= startDate && activity.Entrada <= endDate)
                        .Group(
                            key => key.Cliente.Membresia,
                            group => new membershipsCount
                            {
                                nombre = group.Key,
                                conteo = group.Count()
                            }).SortByDescending(x => x.conteo)
                              .ToListAsync();

                return memberships;
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al contar clientes por sucursal. {ex}");
            }
        }

        public async Task<IEnumerable<membershipsCount>> getMostPopularMemberships(string branchId)
        {
            try
            {
                if (!string.IsNullOrEmpty(branchId))
                {
                    if (!await IsBranchIdAlreadyExists(branchId))
                        throw new ArgumentException($"La sucursal: {branchId} no existe.");

                    var startDate = DateTime.UtcNow.AddDays(-31);
                    var endDate = DateTime.UtcNow;

                    var branchEspecific = await _activityCenterCollection.Aggregate()
                        .Match(activity => activity.Entrada >= startDate && activity.Entrada <= endDate && activity.Sucursal.Sucursal_Id == branchId)
                        .Group(
                            key => key.Cliente.Membresia,
                            group => new membershipsCount
                            {
                                nombre = group.Key,
                                conteo = group.Count()
                            }).SortByDescending(x => x.conteo)
                              .ToListAsync();

                    return branchEspecific;
                }

                return await getMembershipsCount(); ;
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al contar clientes por sucursal. {ex}");
            }
        }
    }
}

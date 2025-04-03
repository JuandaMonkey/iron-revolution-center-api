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

        public StatisticsService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
            _branchesCollection = _mongoDatabase.GetCollection<BranchesModel>("Branches");
            _activityCenterCollection = _mongoDatabase.GetCollection<Activity_CenterModel>("Activity_Center");
            _employeesCollection = _mongoDatabase.GetCollection<EmployeesModel>("Employees");
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

        public async Task<activeClientsDTO> getActiveClients(string branchId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                if (!await IsBranchIdAlreadyExists(branchId))
                    throw new ArgumentException($"La sucursal: {branchId} no existe.");
            }
            try
            {
                var filterBuilder = Builders<Activity_CenterModel>.Filter;
                var filter = new List<FilterDefinition<Activity_CenterModel>>();

                if (!string.IsNullOrEmpty(branchId))
                    filter.Add(filterBuilder.Eq(activity_center => activity_center.Sucursal.Sucursal_Id, branchId));

                DateTime entry = DateTime.UtcNow;
                DateTime exit = DateTime.UtcNow.AddHours(24);

                filter.Add(filterBuilder.Gte(activity_center => activity_center.Entrada, entry));
                filter.Add(filterBuilder.Lte(activity_center => activity_center.Salida, exit));

                var dateFilter = filterBuilder.Or(
                    filterBuilder.And(
                        filterBuilder.Gte(activity_center => activity_center.Entrada, entry),
                        filterBuilder.Lte(activity_center => activity_center.Entrada, exit)
                    ),
                    filterBuilder.And(
                        filterBuilder.Gte(activity_center => activity_center.Salida, entry),
                        filterBuilder.Lte(activity_center => activity_center.Salida, exit)
                    )
                );

                filter.Add(dateFilter);

                var filters = filter.Any() ? filterBuilder.And(filter) : filterBuilder.Empty;

                var active = await _activityCenterCollection
                    .CountDocumentsAsync(filters);

                return new activeClientsDTO
                {
                    Active_Clients = active
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
    }
}

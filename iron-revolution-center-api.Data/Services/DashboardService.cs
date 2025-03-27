using iron_revolution_center_api.Data.Interfaces;
using iron_revolution_center_api.DTOs.Membership;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Services
{
    public class DashboardService : iDashboardService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<BranchesModel> _branchesCollection;
        private readonly IMongoCollection<MembershipsModel> _membershipCollection;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;
        private readonly IMongoCollection<EmployeesModel> _employeesCollection;
        private readonly IMongoCollection<Activity_CenterModel> _activityCenterCollection;

        // method to exclude _id field
        private static ProjectionDefinition<BranchesModel> ExcludeIdProjection()
        {
            return Builders<BranchesModel>.Projection.Exclude("_id");
        }
        private static ProjectionDefinition<MembershipsModel> ExcludeIdProjectionMemberships()
        {
            return Builders<MembershipsModel>.Projection.Exclude("_id");
        }
        private static ProjectionDefinition<ClientsModel> ExcludeIdProjectionClient()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }
        private static ProjectionDefinition<EmployeesModel> ExcludeIdProjectionEmployee()
        {
            return Builders<EmployeesModel>.Projection.Exclude("_id");
        }

        public DashboardService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _branchesCollection = _mongoDatabase.GetCollection<BranchesModel>("Branches");
            _membershipCollection = _mongoDatabase.GetCollection<MembershipsModel>("Memberships");
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
            _employeesCollection = _mongoDatabase.GetCollection<EmployeesModel>("Employees");
            _activityCenterCollection = _mongoDatabase.GetCollection<Activity_CenterModel>("Activity_Center");
        }
        #endregion

        #region Validations
        private async Task<bool> IsBranchIdAlreadyUsed(string branchId)
        {
            try
            {
                var branchExists = await _branchesCollection
                    .CountDocumentsAsync(branch => branch.Sucursal_Id == branchId);
                return branchExists > 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<long> CountClients()
        {
            return await _clientsCollection.CountDocumentsAsync(FilterDefinition<ClientsModel>.Empty);
        }

        private async Task<long> CountActiveClients(string branchId)
        {
            var startDay = DateTime.UtcNow.Date;
            var endDay = startDay.AddDays(1).AddTicks(-1);

            if (branchId == "Todos" || string.IsNullOrEmpty(branchId))
            {
                return await _activityCenterCollection
                    .CountDocumentsAsync(clients => clients.Entrada >= startDay && clients.Entrada <= endDay);
            }
            else
            {
                return await _activityCenterCollection
                    .CountDocumentsAsync(clients => clients.Sucursal.Sucursal_Id == branchId && clients.Entrada >= startDay && clients.Entrada <= endDay);
            }
        }

        public async Task<string> MostFrecuentedBranch(string branchId)
        {
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(-7);

            var dateFilter = Builders<Activity_CenterModel>.Filter.And(
                Builders<Activity_CenterModel>.Filter.Gte(activityCenter => activityCenter.Entrada, startDate),
                Builders<Activity_CenterModel>.Filter.Lte(activityCenter => activityCenter.Salida, today));

            if (branchId != "Todos" && !string.IsNullOrEmpty(branchId))
            {
                var branchFilter = Builders<Activity_CenterModel>.Filter.Eq(activityCenter => activityCenter.Sucursal.Sucursal_Id, branchId);
                var combinedFilter = Builders<Activity_CenterModel>.Filter.And(dateFilter, branchFilter);
                var count = await _activityCenterCollection.CountDocumentsAsync(combinedFilter);
                return count.ToString();
            }

            var mostFrequentedBranch = await _activityCenterCollection
                .Aggregate()
                .Match(dateFilter)
                .Group(activityCenter => activityCenter.Sucursal.Nombre, branchesCount => new
                {
                    Branch = branchesCount.Key,
                    Count = branchesCount.Count()
                })
                .SortByDescending(branchesCount => branchesCount.Count)
                .Limit(1)
                .FirstOrDefaultAsync();

            return mostFrequentedBranch?.Branch;
        }

        private async Task<long> CountEmployees(string branchId)
        {
            if (branchId == "Todos" || string.IsNullOrEmpty(branchId))
            {
                return await _employeesCollection.CountDocumentsAsync(FilterDefinition<EmployeesModel>.Empty);
            } else {
                return await _employeesCollection.CountDocumentsAsync(employees => employees.Sucursal.Sucursal_Id == branchId);
            }
        }
        #endregion

        public async Task<DashboardModel> Dashboard(string branchId)
        {
            try
            {
                var registered_clients = await CountClients();
                var active_clients = await CountActiveClients(branchId);
                var registered_employees = await CountEmployees(branchId);
                var most_frecuented_branch = await MostFrecuentedBranch(branchId);

                return new DashboardModel
                {
                    Clientes_Registrados = registered_clients,
                    Clientes_Activos = active_clients,
                    Empleados_Registrados = registered_employees,
                    Sucursal_Mas_Frecuntada = most_frecuented_branch
                };
            } catch (Exception ex) {
                throw new ArgumentException($"Error: {ex.Message}");
            }
        }
    }
}

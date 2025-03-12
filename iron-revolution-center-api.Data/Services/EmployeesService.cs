using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Staff;
using iron_revolution_center_api.DTOs.User;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Service
{
    public class EmployeesService : iEmployeesService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<EmployeesModel> _employeesCollection;
        private readonly IMongoCollection<RegisterEmployeeDTO> _registerEmployeeCollection;
        private readonly IMongoCollection<ModifyEmployeeDTO> _modifyEmployeeCollection;
        private readonly IMongoCollection<BranchesModel> _branchesCollection;

        // method to exclude _id field
        private static ProjectionDefinition<EmployeesModel> ExcludeIdProjection()
        {
            return Builders<EmployeesModel>.Projection.Exclude("_id");
        }

        // method to exclude _id field
        private static ProjectionDefinition<BranchesModel> ExcludeIdProjectionBranches()
        {
            return Builders<BranchesModel>.Projection.Exclude("_id");
        }

        public EmployeesService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _employeesCollection = _mongoDatabase.GetCollection<EmployeesModel>("Employees");
            _registerEmployeeCollection = _mongoDatabase.GetCollection<RegisterEmployeeDTO>("Employees");
            _modifyEmployeeCollection = _mongoDatabase.GetCollection<ModifyEmployeeDTO>("Employees");
            _branchesCollection = _mongoDatabase.GetCollection<BranchesModel>("Branches");
        }
        #endregion

        #region Validations
        private async Task<bool> IsEmployeeRegistered(string NIP)
        {
            try
            {
                // check employee
                var NIPIsAlreadyUsed = await _employeesCollection
                    .CountDocumentsAsync(staff => staff.NIP == NIP);

                // validate existence
                return NIPIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> IsPhoneAlreadyUsed(string employeePhone)
        {
            try
            {
                // check employee
                var phoneIsAlreadyUsed = await _employeesCollection
                    .CountDocumentsAsync(staff => staff.Celular == employeePhone);

                // validate existence
                return phoneIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> IsBranchRegistered(string branchId)
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
        #endregion

        #region ListEmployees
        public async Task<IEnumerable<EmployeesModel>> ListEmployees()
        {
            try
            {
                // get employees
                return await _employeesCollection
                    .Find(FilterDefinition<EmployeesModel>.Empty)
                    .Project<EmployeesModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar empleados. {ex}");
            }
        }
        #endregion

        #region GetEmployeeByNIP
        public async Task<EmployeesModel> GetEmployeeByNIP(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                throw new ArgumentException($"El NIP del trabajador no puede estar vacio.");
            if (!await IsEmployeeRegistered(NIP)) 
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // get employee
                var employee = await _employeesCollection
                    .Find(employee => employee.NIP == NIP)
                    .Project<EmployeesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // get branch 
                var branch = await _branchesCollection
                    .Find(branch => branch.Sucursal_Id == employee.Sucursal.Sucursal_Id)
                    .Project<BranchesModel>(ExcludeIdProjectionBranches())
                    .FirstOrDefaultAsync();

                var employeeInformation = new EmployeesModel
                {
                    NIP = employee.NIP,
                    Foto = employee.Foto,
                    Nombre_Completo = employee.Nombre_Completo,
                    Celular = employee.Celular,
                    Sucursal = branch
                };

                return employeeInformation;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar empleado. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterEmployee
        public async Task<EmployeesModel> RegisterEmployee(RegisterEmployeeDTO employeeDTO)
        {
            if (string.IsNullOrEmpty(employeeDTO.Celular))
                throw new ArgumentException($"El número de celular no puede estar vacio.");
            if (await IsPhoneAlreadyUsed(employeeDTO.Celular))
                throw new ArgumentException($"Número de celular: {employeeDTO.Celular} ya en uso.");
            if (string.IsNullOrEmpty(employeeDTO.Sucursal.Sucursal_Id)) 
                throw new ArgumentException($"La sucursal no puede estar vacio.");
            if (!await IsBranchRegistered(employeeDTO.Sucursal.Sucursal_Id))
                throw new ArgumentException($"La sucursal con ID: {employeeDTO.Sucursal.Sucursal_Id} no existe.");
            try
            {
                // generate a unique nip
                string NIP;
                do
                {
                    string num = new Random().Next(1, 1000).ToString("D3");
                    NIP = $"EM{num}";
                } while (await IsEmployeeRegistered(NIP));

                // get branches
                var branches = await _branchesCollection
                    .Find(branch => branch.Sucursal_Id == employeeDTO.Sucursal.Sucursal_Id)
                    .Project<BranchesModel>(ExcludeIdProjectionBranches())
                    .FirstOrDefaultAsync();

                employeeDTO.NIP = NIP;
                employeeDTO.Sucursal = branches;

                // register employee
                var newEmployee = new RegisterEmployeeDTO
                {
                    NIP = employeeDTO.NIP,
                    Foto = employeeDTO.Foto,
                    Nombre_Completo = employeeDTO.Nombre_Completo,
                    Celular = employeeDTO.Celular,
                    Sucursal = branches
                };

                // check if is not null
                if (newEmployee == null)
                    throw new ArgumentException("Registro fallido.");

                // register
                await _registerEmployeeCollection.InsertOneAsync(newEmployee);

                // employee
                return await GetEmployeeByNIP(NIP);
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar empleado. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyEmployees
        public async Task<EmployeesModel> ModifyEmployee(string NIP, ModifyEmployeeDTO employeeDTO)
        {
            if (string.IsNullOrEmpty(NIP))
                throw new ArgumentException("El NIP del empleado no puede estar vacio.");
            if (!await IsEmployeeRegistered(NIP)) 
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            if (string.IsNullOrEmpty(employeeDTO.Sucursal.Sucursal_Id))
                throw new ArgumentException($"La sucursal no puede estar vacio.");
            if (!await IsBranchRegistered(employeeDTO.Sucursal.Sucursal_Id))
                throw new ArgumentException($"La sucursal con ID: {employeeDTO.Sucursal.Sucursal_Id} no existe.");
            try
            {
                // create update definitions 
                var updateBuilder = Builders<EmployeesModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<EmployeesModel>>();

                // modify not null field
                if (employeeDTO.Foto != null) // photo
                    updateDefinitions.Add(updateBuilder
                                     .Set(employee => employee.Foto, employeeDTO.Foto));
                if (!string.IsNullOrEmpty(employeeDTO.Nombre_Completo)) // full name
                    updateDefinitions.Add(updateBuilder
                                     .Set(staff => staff.Nombre_Completo, employeeDTO.Nombre_Completo));
                if (!string.IsNullOrEmpty(employeeDTO.Celular)) // phone
                {
                    if (await IsPhoneAlreadyUsed(employeeDTO.Celular)) // field verification
                        throw new ArgumentException($"Número de celular: {employeeDTO.Celular} ya en uso.");
                    updateDefinitions.Add(updateBuilder
                                     .Set(staff => staff.Celular, employeeDTO.Celular));
                }
                if (employeeDTO.Sucursal.Sucursal_Id != null) // branch
                {
                    // check branch exists
                    if (!await IsBranchRegistered(employeeDTO.Sucursal.Sucursal_Id))
                        throw new ArgumentException($"La sucursal con ID: {employeeDTO.Sucursal} no existe.");

                    // get branches
                    var branch = await _branchesCollection
                        .Find(branch => branch.Sucursal_Id == employeeDTO.Sucursal.Sucursal_Id)
                        .Project<BranchesModel>(ExcludeIdProjectionBranches())
                        .FirstOrDefaultAsync();

                    updateDefinitions.Add(updateBuilder
                                     .Set(employee => employee.Sucursal, new BranchesModel
                                     {
                                         Sucursal_Id = branch.Sucursal_Id,
                                         Nombre = branch.Nombre,
                                         Ubicacion = branch.Ubicacion
                                     }));
                }

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<EmployeesModel>
                    .Filter
                    .Eq(employee => employee.NIP, NIP);
                // result of the modify
                var update = await _employeesCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new ArgumentException("Error al modificar empleado");

                // employee
                return await GetEmployeeByNIP(NIP);
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al modificar empleado. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteEmployee
        public async Task<EmployeesModel> DeleteEmployee(string NIP)
        {
            if (string.IsNullOrEmpty(NIP))
                throw new ArgumentException("El NIP del empleado no puede estar vacio.");
            if (!await IsEmployeeRegistered(NIP))
                throw new ArgumentException($"El NIP: {NIP} no existe.");
            try
            {
                // employee
                var employee = await GetEmployeeByNIP(NIP);

                // check if is not null
                if (employee == null)
                    throw new ArgumentException("Empleado no encontrado.");

                // delete
                await _employeesCollection.DeleteOneAsync(employee => employee.NIP == NIP);

                // employee
                return employee;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar trabajador. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}

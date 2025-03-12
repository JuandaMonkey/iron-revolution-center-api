using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Role;
using iron_revolution_center_api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace iron_revolution_center_api.Data.Service
{
    // role services
    public class RolesService : iRolesInterface
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<RolesModel> _rolesCollection;
        private readonly IMongoCollection<InsertRoleDTO> _insertRoleCollection;
        private readonly IMongoCollection<ModifyRoleDTO> _modifyRoleCollection;

        // method to exclude _id field
        private static ProjectionDefinition<RolesModel> ExcludeIdProjection()
        {
            return Builders<RolesModel>.Projection.Exclude("_id");
        }

        public RolesService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _rolesCollection = _mongoDatabase.GetCollection<RolesModel>("Roles");
            _insertRoleCollection = _mongoDatabase.GetCollection<InsertRoleDTO>("Roles");
            _modifyRoleCollection = _mongoDatabase.GetCollection<ModifyRoleDTO>("Roles");
        }
        #endregion

        #region Validations
        private async Task<bool> IsRoleIdAlreadyUsed(string roleId)
        {
            try
            {
                // check role id
                var isIdUsed = await _rolesCollection
                    .CountDocumentsAsync(role => role.Rol_Id == roleId);

                // validate existence
                return isIdUsed > 0;
            } catch {
                return false;
            }
        }

        private async Task<bool> IsRoleNameAlreadyUsed(string roleName)
        {
            try
            {
                // check role name
                var isNameUsed = await _rolesCollection
                    .CountDocumentsAsync(role => role.Nombre == roleName);

                // validate existence
                return isNameUsed > 0;
            } catch  {
                return false;
            }
        }
        #endregion

        #region ListRoles
        public async Task<IEnumerable<RolesModel>> ListRoles()
        {
            try
            {
                // get roles
                return await _rolesCollection
                    .Find(FilterDefinition<RolesModel>.Empty)
                    .Project<RolesModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar roles. {ex}");
            }
        }
        #endregion

        #region InsertRole
        public async Task<InsertRoleDTO> InsertRole(InsertRoleDTO roleDTO)
        {
            if (string.IsNullOrEmpty(roleDTO.Nombre))
                throw new ArgumentException($"El nombre no puede estar vacío.");
            if (await IsRoleNameAlreadyUsed(roleDTO.Nombre))
                throw new ArgumentException($"Nombre de rol: {roleDTO.Nombre} ya en uso");
            try
            {
                // generate a unique identification
                string roleId;
                do
                {
                    string num = new Random().Next(1, 1000).ToString("D3");
                    roleId = $"R{num}";
                } while (await IsRoleIdAlreadyUsed(roleId));

                roleDTO.Rol_Id = roleId;

                Console.WriteLine($"ID: {roleId}"); // id generated

                // insert model
                var newRole = new InsertRoleDTO
                {
                    Rol_Id = roleDTO.Rol_Id,
                    Nombre = roleDTO.Nombre
                };

                Console.WriteLine($"Nuevo Rol: {System.Text.Json.JsonSerializer.Serialize(newRole)}"); // new rol

                // check if is not null
                if (newRole == null)
                    throw new ArgumentException($"Inserción fallida.");

                // insert
                await _insertRoleCollection.InsertOneAsync(newRole);

                // role
                return newRole;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al insertar rol. {ex}");
            } catch (ArgumentException ex) {
                // in case of error 
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyRole
        public async Task<RolesModel> ModifyRole(string roleId, ModifyRoleDTO roleDTO)
        {
            if (string.IsNullOrEmpty(roleId)) 
                throw new ArgumentException($"El ID no puede estar vacío.");
            if (!await IsRoleIdAlreadyUsed(roleId)) 
                throw new ArgumentException($"El ID {roleId} no existe.");
            if (string.IsNullOrEmpty(roleDTO.Nombre))
                throw new ArgumentException($"El nombre no puede estar vacío.");
            if (await IsRoleNameAlreadyUsed(roleDTO.Nombre))
                throw new ArgumentException($"El nombre {roleDTO.Nombre} esta en uso.");
            try
            {
                // create update definitions
                var updateBuilder = Builders<RolesModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<RolesModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(roleDTO.Nombre)) // name
                    updateDefinitions.Add(updateBuilder
                                     .Set(membership => membership.Nombre, roleDTO.Nombre));

                // verification
                if (!updateDefinitions.Any())
                    throw new ArgumentException("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<RolesModel>
                    .Filter
                    .Eq(role => role.Rol_Id, roleId);
                var update = await _rolesCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new Exception("Error al modificar rol.");

                // find role
                var role = await _rolesCollection
                    .Find(filter)
                    .Project<RolesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // role
                return role;
            } catch (MongoException ex) {
                // in case of erro
                throw new InvalidOperationException($"Error al modificar rol con el ID {roleId}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteRole
        public async Task<RolesModel> DeleteRole(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentException($"El ID no puede estar vacío.");
            if (!await IsRoleIdAlreadyUsed(roleId))
                throw new ArgumentException($"El ID {roleId} no existe.");
            try
            {
                // role
                var role = await _rolesCollection
                    .Find(role => role.Rol_Id == roleId)
                    .Project<RolesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // delete role
                var delete = await _rolesCollection
                    .DeleteOneAsync(role => role.Rol_Id == roleId);

                // check 
                if (delete.DeletedCount == 0)
                    throw new Exception("Error al eliminar rol.");

                // role
                return role;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al eliminar al rol con el ID: {roleId}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}

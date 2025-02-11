using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Client;
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
    public class RolesService : iRolesService
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
        private async Task<bool> ValidateRoleID(string roleID)
        {
            try
            {
                // check role name
                var IDIsAlreadyUsed = await _rolesCollection
                    .CountDocumentsAsync(role => role.Role_ID == roleID);

                // validate existence
                return IDIsAlreadyUsed > 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ValidateRoleName(string name)
        {
            try
            {
                // check role name
                var NameIsAlreadyUsed = await _rolesCollection
                    .CountDocumentsAsync(role => role.Name == name);

                // validate existence
                return NameIsAlreadyUsed > 0;
            } catch {
                return false;
            }
        }
        #endregion

        #region ListRoles
        public async Task<IEnumerable<RolesModel>> ListRoles()
        {
            try
            {
                // get the collection
                return await _rolesCollection
                    .Find(FilterDefinition<RolesModel>.Empty)
                    .Project<RolesModel>(ExcludeIdProjection())
                    .ToListAsync();
            }
            catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar roles. {ex}");
            }
        }
        #endregion

        #region GetRoleByID
        public async Task<IEnumerable<RolesModel>> GetRoleByID(string roleID)
        {
            if (string.IsNullOrEmpty(roleID)) // field verification
                throw new ArgumentException("El ID no puede estar vacío.", nameof(roleID));
            if (await ValidateRoleID(roleID) == true) // field verification
                throw new ArgumentException($"El ID: {roleID} no existe.");
            try
            {
                // get the collection
                return await _rolesCollection
                    .Find(role => role.Role_ID == roleID)
                    .Project<RolesModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al al mostrar role. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region InsertRole
        public async Task<InsertRoleDTO> InsertRole(InsertRoleDTO roleDTO)
        {
            if (string.IsNullOrEmpty(roleDTO.Name)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(roleDTO.Name)}");
            if (await ValidateRoleName(roleDTO.Name) == true) // fiel verification
                throw new ArgumentException($"Nombre de rol: {roleDTO.Name} ya en uso");
            try
            {
                // generate a unique identification
                string id;
                string num;
                do
                {
                    num = new Random().Next(1, 1000).ToString("D3");
                    id = $"R{num}";
                }
                // check if identification is already used
                while (await ValidateRoleID(id) == true);

                // insert model
                var newRole = new InsertRoleDTO
                {
                    Role_ID = id,
                    Name = roleDTO.Name
                };

                // check if is not null
                if (newRole == null)
                    throw new Exception($"Inserción fallida.");

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
            } catch (Exception ex) {
                // in case of error 
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyRole
        public async Task<RolesModel> ModifyRole(string roleID, ModifyRoleDTO roleDTO)
        {
            if (string.IsNullOrEmpty(roleID)) // field verification
                throw new ArgumentException($"El ID no puede estar vacío. {nameof(roleID)}");
            if (await ValidateRoleID(roleID) == false) // field verification
                throw new ArgumentException($"El ID {roleID} no existe.");
            if (string.IsNullOrEmpty(roleDTO.Name)) // field verification
                throw new ArgumentException($"El nombre no puede estar vacío. {nameof(roleDTO.Name)}");
            if (await ValidateRoleName(roleDTO.Name) == true) // field verification
                throw new ArgumentException($"El nombre {roleDTO.Name} esta en uso.");
            try
            {
                // create update definitions
                var updateBuilder = Builders<RolesModel>.Update;
                var updateDefinitions = new List<UpdateDefinition<RolesModel>>();

                // modify not null field
                if (!string.IsNullOrEmpty(roleDTO.Name)) // name
                    updateDefinitions.Add(updateBuilder
                                     .Set(membership => membership.Name, roleDTO.Name));

                // verification
                if (!updateDefinitions.Any())
                    throw new Exception("No se proporcionaron campos válidos para modificar.");

                // combine to a single
                var combine = updateBuilder.Combine(updateDefinitions);

                // modify
                var filter = Builders<RolesModel>
                    .Filter
                    .Eq(role => role.Role_ID, roleID);
                var update = await _rolesCollection
                    .UpdateOneAsync(filter, combine);
                // check if the update was successful
                if (update.ModifiedCount == 0)
                    throw new Exception("Error al modificar rol.");

                // find role
                RolesModel role = await _rolesCollection
                    .Find(filter)
                    .Project<RolesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // role
                return role;
            }
            catch (MongoException ex) {
                // in case of erro
                throw new InvalidOperationException($"Error al modificar rol con el ID {roleID}. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            } catch (Exception ex) {
                // in case of error
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteRole
        public async Task<RolesModel> DeleteRole(string roleID)
        {
            if (string.IsNullOrEmpty(roleID)) // field verification
                throw new ArgumentException($"El ID no puede estar vacío. {nameof(roleID)}");
            if (await ValidateRoleID(roleID) == false) // field verification
                throw new ArgumentException($"El ID {roleID} no existe.");
            try
            {
                // role
                RolesModel role = await _rolesCollection
                    .Find(role => role.Role_ID == roleID)
                    .Project<RolesModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // delete role
                var delete = await _rolesCollection
                    .DeleteOneAsync(role => role.Role_ID == roleID);

                // check 
                if (delete.DeletedCount == 0)
                    throw new Exception("Error al eliminar rol.");

                // role
                return role;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al eliminar al rol con el ID: {roleID}. {ex}");
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

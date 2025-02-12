using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.DTOs.User;
using iron_revolution_center_api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace iron_revolution_center_api.Data.Service
{
    public class UsersService : iUsersService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<RolesModel> _roleCollection;
        private readonly IMongoCollection<UsersModel> _usersCollection;
        private readonly IMongoCollection<RegisterUserDTO> _registerUserCollection;

        // method to exclude _id field
        private static ProjectionDefinition<UsersModel> ExcludeIdProjection()
        {
            return Builders<UsersModel>.Projection.Exclude("_id");
        }

        public UsersService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _roleCollection = _mongoDatabase.GetCollection<RolesModel>("Roles");
            _usersCollection = _mongoDatabase.GetCollection<UsersModel>("Users");
            _registerUserCollection = _mongoDatabase.GetCollection<RegisterUserDTO>("Users");
        }
        #endregion

        #region Validations
        private async Task<bool> ValidateUserName(string userName)
        {
            try
            {
                // check user name
                var UserNameIsAlreadyUsed = await _usersCollection
                    .CountDocumentsAsync(user => user.User_Name == userName);

                // validate existence
                return UserNameIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> ValidateRoleExists(string roleID)
        {
            try
            {
                // check user role
                var RoleExists = await _usersCollection
                    .CountDocumentsAsync(user => user.Role == roleID);

                // validate existence
                return RoleExists > 0;
            } catch {
                // if not exist
                return false;
            }
        }

        private async Task<string> GetRoleNameByID(string roleID)
        {
            try
            {
                // check user role
                var role = await _roleCollection
                    .Find(role => role.Role_ID == roleID)
                    .FirstOrDefaultAsync();

                // validate existence
                return role?.Name;
            } catch {
                // if not exist
                return null;
            }
        }
        #endregion

        #region ListUsers
        public async Task<IEnumerable<UsersModel>> ListUsers()
        {
            try
            {
                // get the collection users
                return await _usersCollection
                    .Find(FilterDefinition<UsersModel>.Empty)
                    .Project<UsersModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al mostrar usuarios. {ex}");
            }
        }
        #endregion

        #region ListUsersByRole
        public async Task<IEnumerable<UsersModel>> ListUsersByRole(string roleID)
        {
            if (string.IsNullOrEmpty(roleID)) // field verification
                throw new ArgumentException("El role no puede estar vacío.", nameof(roleID));
            if (!await ValidateRoleExists(roleID)) // field verification
            {
                var roleName = await GetRoleNameByID(roleID);
                throw new ArgumentException($"El role: {GetRoleNameByID(roleID)} no existe.");
            }
            try
            {
                // get role name
                var roleName = await GetRoleNameByID(roleID);

                // get users
                return await _usersCollection
                    .Find(user => user.Role == roleID)
                    .Project<UsersModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al al mostrar usuarios. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region GetUsersByUserName
        public async Task<IEnumerable<UsersModel>> GetUsersByUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) // field verification
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(userName));
            try
            {
                // get the collection
                return await _usersCollection
                    .Find(user => user.User_Name == userName)
                    .Project<UsersModel>(ExcludeIdProjection())
                    .ToListAsync();
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al al mostrar usuario. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterUser
        public async Task<RegisterUserDTO> RegisterUser(RegisterUserDTO userDTO)
        {
            if (string.IsNullOrEmpty(userDTO.User_Name)) // field verification
                throw new ArgumentException($"El nombre de usuario no puede estar vacío. {nameof(userDTO.User_Name)}");
            if (await ValidateUserName(userDTO.User_Name) == true) // fiel verification
                throw new ArgumentException($"Nombre de ususario: {userDTO.User_Name} ya en uso");
            if (string.IsNullOrEmpty(userDTO.Password)) // field verification
                throw new ArgumentException($"La contraseña no puede estar vacío. {nameof(userDTO.Password)}");
            if (string.IsNullOrEmpty(userDTO.Role)) // field verification
                throw new ArgumentException($"El role no puede estar vacío. {nameof(userDTO.User_Name)}");
            if (await ValidateRoleExists(userDTO.Role) == false) // fiel verification
                throw new ArgumentException($"El rol: {userDTO.Role} no existe.");
            try
            {
                // register user
                var newUser = new RegisterUserDTO
                {
                    User_Name = userDTO.User_Name,
                    Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                    Role = userDTO.Role
                };

                // check if is not null
                if (newUser == null)
                    throw new Exception($"Registro fallido.");

                // register
                await _registerUserCollection.InsertOneAsync(newUser);

                // user
                return newUser;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar usuario. {ex}");
            } catch (ArgumentException ex) {
                // in case of error 
                throw new ArgumentException($"Error: {ex}");
            } catch (Exception ex) {
                // in case of error 
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region RegisterUserClient
        public async Task<RegisterUserDTO> RegisterUserClient(RegisterUserDTO userDTO)
        {
            if (string.IsNullOrEmpty(userDTO.User_Name)) // field verification
                throw new ArgumentException($"El nombre de usuario no puede estar vacío. {nameof(userDTO.User_Name)}");
            if (await ValidateUserName(userDTO.User_Name) == true) // fiel verification
                throw new ArgumentException($"Nombre de ususario: {userDTO.User_Name} ya en uso");
            if (string.IsNullOrEmpty(userDTO.Password)) // field verification
                throw new ArgumentException($"La contraseña no puede estar vacío. {nameof(userDTO.Password)}");
            try
            {
                // register user
                var newUser = new RegisterUserDTO
                {
                    User_Name = userDTO.User_Name,
                    Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                    Role = "R332"
                };

                // check if is not null
                if (newUser == null)
                    throw new Exception($"Registro fallido.");

                // register
                await _registerUserCollection.InsertOneAsync(newUser);

                // client
                return newUser;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar usuario. {ex}");
            } catch (ArgumentException ex) {
                // in case of error 
                throw new ArgumentException($"Error: {ex}");
            } catch (Exception ex) {
                // in case of error 
                throw new Exception($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyUserName
        public async Task<UsersModel> ModifyUserName(string userName, ModifyUserNameDTO userNameDTO)
        {
            if (string.IsNullOrEmpty(userName)) // field verification
                throw new ArgumentException("El ID de usuario no puede estar vacío.", nameof(userName));
            if (string.IsNullOrEmpty(userNameDTO.User_Name)) // field verification
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(userNameDTO.User_Name));
            if (await ValidateUserName(userName) == false) // field verification
                throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
            try
            {
                // check if user name is already used
                if (await ValidateUserName(userNameDTO.User_Name))
                    throw new ArgumentException($"El nombre de usuario {userNameDTO.User_Name} ya está en uso.");

                // find a user
                var user = await _usersCollection
                    .Find(user => user.User_Name == userName)
                    .FirstOrDefaultAsync();

                // check if is not null
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado.");

                // changes
                user.User_Name = userNameDTO.User_Name;

                // modify
                await _usersCollection.ReplaceOneAsync(user => user.User_Name == userName, user);

                // Retornar el modelo actualizado
                return user;
            } catch (MongoException ex) {
                // En caso de error
                throw new InvalidOperationException($"Error al modificar el nombre de usuario. {ex}");
            } catch (ArgumentException ex) {
                // En caso de error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyPassword
        public async Task<UsersModel> ModifyPassword(string userName, ModifyPassworDTO passwordDTO)
        {
            if (string.IsNullOrEmpty(userName)) // field verification
                throw new ArgumentException("El ID de usuario no puede estar vacío.", nameof(userName));
            if (string.IsNullOrEmpty(passwordDTO.Password)) // field verification
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(passwordDTO.Password));
            if (await ValidateUserName(userName) == false) // field verification
                throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
            try
            {
                // find a user
                var user = await _usersCollection
                    .Find(user => user.User_Name == userName)
                    .FirstOrDefaultAsync();

                // check if is not null
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado.");

                // changes
                user.Password = BCrypt.Net.BCrypt.HashPassword(passwordDTO.Password);

                // modify
                await _usersCollection.ReplaceOneAsync(user => user.User_Name == userName, user);

                // user
                return user;
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al modificar el nombre de usuario. {ex}");
            } catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region DeleteUser
        public async Task<UsersModel> DeleteUser(string userName)
        {
            if (string.IsNullOrEmpty(userName)) // field verification
                throw new ArgumentException("El ID de usuario no puede estar vacío.", nameof(userName));
            if (await ValidateUserName(userName) == false) // field verification
                throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
            try
            {
                // find a user
                var user = await _usersCollection
                    .Find(user => user.User_Name == userName)
                    .FirstOrDefaultAsync();

                // check if is not null
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado.");

                // delete
                await _usersCollection.DeleteOneAsync(user => user.User_Name == userName);

                // user
                return user;
            }
            catch (MongoException ex)
            {
                // in case of error
                throw new InvalidOperationException($"Error al modificar el nombre de usuario. {ex}");
            }
            catch (ArgumentException ex) {
                // in case of error
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion
    }
}

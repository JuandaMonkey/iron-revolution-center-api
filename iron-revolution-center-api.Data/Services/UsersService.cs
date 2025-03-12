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
        //    #region MongoDB Configuration
        //    private readonly IMongoDatabase _mongoDatabase;
        //    private readonly IMongoCollection<UsersModel> _usersCollection;
        //    private readonly IMongoCollection<Models.RolesModel> _roleCollection;
        //    private readonly IMongoCollection<StaffModel> _staffCollection;
        //    private readonly IMongoCollection<ClientsModel> _clientsCollection;
        //    private readonly IMongoCollection<RegisterUserDTO> _registerUserCollection;

        //    // method to exclude _id field
        //    private static ProjectionDefinition<UsersModel> ExcludeIdProjection()
        //    {
        //        return Builders<UsersModel>.Projection.Exclude("_id");
        //    }

        //    private static ProjectionDefinition<Models.RolesModel> ExcludeIdProjectionRole()
        //    {
        //        return Builders<Models.RolesModel>.Projection.Exclude("_id");
        //    }

        //    private static ProjectionDefinition<StaffModel> ExcludeIdProjectionStaff()
        //    {
        //        return Builders<StaffModel>.Projection.Exclude("_id");
        //    }

        //    private static ProjectionDefinition<ClientsModel> ExcludeIdProjectionClients()
        //    {
        //        return Builders<ClientsModel>.Projection.Exclude("_id");
        //    }

        //    public UsersService(IMongoDatabase mongoDatabase)
        //    {
        //        _mongoDatabase = mongoDatabase;
        //        _usersCollection = _mongoDatabase.GetCollection<UsersModel>("Users");
        //        _roleCollection = _mongoDatabase.GetCollection<Models.RolesModel>("Roles");
        //        _staffCollection = _mongoDatabase.GetCollection<StaffModel>("Staff");
        //        _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
        //        _registerUserCollection = _mongoDatabase.GetCollection<RegisterUserDTO>("Users");
        //    }
        //    #endregion

        //    #region Validations
        //    private async Task<bool> ValidateUserName(string userName)
        //    {
        //        try
        //        {
        //            // check user name
        //            var UserNameIsAlreadyUsed = await _usersCollection
        //                .CountDocumentsAsync(user => user.Nombre_Usuario == userName);

        //            // validate existence
        //            return UserNameIsAlreadyUsed > 0;
        //        }
        //        catch
        //        {
        //            // if not in used
        //            return false;
        //        }
        //    }

        //    private async Task<bool> ValidateRoleExists(string roleID)
        //    {
        //        try
        //        {
        //            // check user role
        //            var RoleExists = await _usersCollection
        //                .CountDocumentsAsync(user => user.Rol == roleID);

        //            // validate existence
        //            return RoleExists > 0;
        //        }
        //        catch
        //        {
        //            // if not exist
        //            return false;
        //        }
        //    }

        //    //private async Task<string> GetRoleNameByID(string roleID)
        //    //{
        //    //    try
        //    //    {
        //    //        // check user role
        //    //        var role = await _roleCollection
        //    //            .Find(role => role.Rol_ID == roleID)
        //    //            .FirstOrDefaultAsync();

        //    //        // validate existence
        //    //        return role?.Nombre;
        //    //    } catch {
        //    //        // if not exist
        //    //        return null;
        //    //    }
        //    //}
        //    #endregion

        //    #region ListUsers
        //    public async Task<IEnumerable<UsersModel>> ListUsers()
        //    {
        //        try
        //        {
        //            // get the collection users
        //            return await _usersCollection
        //                .Find(FilterDefinition<UsersModel>.Empty)
        //                .Project<UsersModel>(ExcludeIdProjection())
        //                .ToListAsync();
        //        }
        //        catch (MongoException ex)
        //        {
        //            // in case of error
        //            throw new InvalidOperationException($"Error al mostrar usuarios. {ex}");
        //        }
        //    }
        //    #endregion

        //    #region ListUsersByRole
        //    //public async Task<IEnumerable<UsersModel>> ListUsersByRole(string roleID)
        //    //{
        //    //    if (string.IsNullOrEmpty(roleID)) // field verification
        //    //        throw new ArgumentException("El role no puede estar vacío.", nameof(roleID));
        //    //    if (!await ValidateRoleExists(roleID)) // field verification
        //    //    {
        //    //        var roleName = await GetRoleNameByID(roleID);
        //    //        throw new ArgumentException($"El role: {roleName} no existe.");
        //    //    }
        //    //    try
        //    //    {
        //    //        // get role name
        //    //        var roleName = await GetRoleNameByID(roleID);

        //    //        // get users
        //    //        return await _usersCollection
        //    //            .Find(user => user.Role == roleID)
        //    //            .Project<UsersModel>(ExcludeIdProjection())
        //    //            .ToListAsync();
        //    //    } catch (MongoException ex) {
        //    //        // in case of error
        //    //        throw new InvalidOperationException($"Error al al mostrar usuarios. {ex}");
        //    //    } catch (ArgumentException ex) {
        //    //        // in case of error
        //    //        throw new ArgumentException($"Error: {ex}");
        //    //    }
        //    //}
        //    #endregion

        //    #region GetUsersByUserName
        //    //public async Task<UsersModel> GetUsersByUserName(string userName)
        //    //{
        //    //    if (string.IsNullOrEmpty(userName)) // field verification
        //    //        throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(userName));
        //    //    if (!await ValidateUserName(userName)) // field veridication
        //    //        throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
        //    //    try
        //    //    {
        //    //        // get user
        //    //        var user = await _usersCollection
        //    //           .Find(user => user.Nombre_Usuario == userName)
        //    //           .Project<UsersModel>(ExcludeIdProjection())
        //    //           .FirstOrDefaultAsync();

        //    //        // get role name
        //    //        var rolName = await _roleCollection
        //    //            .Find(roleName => roleName.Rol_ID == user.Rol)
        //    //            .Project<RolesService>(ExcludeIdProjectionRole())
        //    //            .FirstOrDefaultAsync();

        //    //        // get nip information
        //    //        PersonalInformationModel nipInformation;
        //    //        // si el primer caracter del nip es "T"
        //    //        if (user?.Informacion_Personal?.NIP?[0] == 'T')
        //    //        {
        //    //            // busca a un trabajador
        //    //            var getStaff = await _staffCollection
        //    //                .Find(staff => staff.NIP == user.Informacion_Personal.NIP)
        //    //                .Project<StaffModel>(ExcludeIdProjectionStaff())
        //    //                .FirstOrDefaultAsync();
        //    //            // almacenalo en la variable
        //    //            nipInformation = getStaff;
        //    //        } else { // sino
        //    //            // busca a un cliente
        //    //            var getClient = await _clientsCollection
        //    //                .Find(client => client.NIP == user.Informacion_Personal.NIP)
        //    //                .Project<ClientsModel>(ExcludeIdProjectionClients())
        //    //                .FirstOrDefaultAsync();
        //    //            // almacenalo en la variable
        //    //            nipInformation = getClient;
        //    //        }

        //    //        // user
        //    //        var userInformation = new UsersModel
        //    //        {
        //    //            Nombre_Usuario = user.Nombre_Usuario,
        //    //            Rol = rolName.Nombre,
        //    //            Informacion_Personal = nipInformation
        //    //        };

        //    //        // get the collection
        //    //        return userInformation;
        //    //    } catch (MongoException ex) {
        //    //        // in case of error
        //    //        throw new InvalidOperationException($"Error al al mostrar usuario. {ex}");
        //    //    } catch (ArgumentException ex) {
        //    //        // in case of error
        //    //        throw new ArgumentException($"Error: {ex}");
        //    //    }
        //    //}
        //    #endregion

        //    #region RegisterUser
        //    public async Task<RegisterUserDTO> RegisterUser(RegisterUserDTO userDTO)
        //    {
        //        if (string.IsNullOrEmpty(userDTO.Nombre_Usuario)) // field verification
        //            throw new ArgumentException($"El nombre de usuario no puede estar vacío. {nameof(userDTO.Nombre_Usuario)}");
        //        if (await ValidateUserName(userDTO.Nombre_Usuario) == true) // fiel verification
        //            throw new ArgumentException($"Nombre de ususario: {userDTO.Nombre_Usuario} ya en uso");
        //        if (string.IsNullOrEmpty(userDTO.Contraseña)) // field verification
        //            throw new ArgumentException($"La contraseña no puede estar vacío. {nameof(userDTO.Contraseña)}");
        //        if (string.IsNullOrEmpty(userDTO.Rol)) // field verification
        //            throw new ArgumentException($"El role no puede estar vacío. {nameof(userDTO.Rol)}");
        //        if (await ValidateRoleExists(userDTO.Rol) == false) // fiel verification
        //            throw new ArgumentException($"El rol: {userDTO.Rol} no existe.");
        //        try
        //        {
        //            // register user
        //            var newUser = new RegisterUserDTO
        //            {
        //                Nombre_Usuario = userDTO.Nombre_Usuario,
        //                Contraseña = BCrypt.Net.BCrypt.HashPassword(userDTO.Contraseña),
        //                Rol = userDTO.Rol,
        //                NIP = userDTO.NIP
        //            };

        //            // check if is not null
        //            if (newUser == null)
        //                throw new Exception($"Registro fallido.");

        //            // register
        //            await _registerUserCollection.InsertOneAsync(newUser);

        //            // get user


        //            // user
        //            return newUser;
        //        }
        //        catch (MongoException ex)
        //        {
        //            // in case of error
        //            throw new InvalidOperationException($"Error al registrar usuario. {ex}");
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            // in case of error 
        //            throw new ArgumentException($"Error: {ex}");
        //        }
        //        catch (Exception ex)
        //        {
        //            // in case of error 
        //            throw new Exception($"Error: {ex}");
        //        }
        //    }
        //    #endregion

        //    #region RegisterUserClient
        //    public async Task<RegisterUserDTO> RegisterUserClient(RegisterUserDTO userDTO)
        //    {
        //        if (string.IsNullOrEmpty(userDTO.Nombre_Usuario)) // field verification
        //            throw new ArgumentException($"El nombre de usuario no puede estar vacío. {nameof(userDTO.Nombre_Usuario)}");
        //        if (await ValidateUserName(userDTO.Nombre_Usuario)) // fiel verification
        //            throw new ArgumentException($"Nombre de ususario: {userDTO.Nombre_Usuario} ya en uso");
        //        if (string.IsNullOrEmpty(userDTO.Contraseña)) // field verification
        //            throw new ArgumentException($"La contraseña no puede estar vacío. {nameof(userDTO.Contraseña)}");
        //        try
        //        {
        //            // register user
        //            var newUser = new RegisterUserDTO
        //            {
        //                Nombre_Usuario = userDTO.Nombre_Usuario,
        //                Contraseña = BCrypt.Net.BCrypt.HashPassword(userDTO.Contraseña),
        //                Rol = "R332"
        //            };

        //            // check if is not null
        //            if (newUser == null)
        //                throw new ArgumentException($"Registro fallido.");

        //            // register
        //            await _registerUserCollection.InsertOneAsync(newUser);

        //            // client
        //            return newUser;
        //        }
        //        catch (MongoException ex)
        //        {
        //            // in case of error
        //            throw new InvalidOperationException($"Error al registrar usuario. {ex}");
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            // in case of error 
        //            throw new ArgumentException($"Error: {ex}");
        //        }
        //    }
        //    #endregion

        //    #region ModifyUserName
        //    public async Task<UsersModel> ModifyUserName(string userName, ModifyUserNameDTO userNameDTO)
        //    {
        //        if (string.IsNullOrEmpty(userName)) // field verification
        //            throw new ArgumentException("El ID de usuario no puede estar vacío.", nameof(userName));
        //        if (string.IsNullOrEmpty(userNameDTO.Nombre_Usuario)) // field verification
        //            throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(userNameDTO.Nombre_Usuario));
        //        if (!await ValidateUserName(userName)) // field verification
        //            throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
        //        try
        //        {
        //            // check if user name is already used
        //            if (await ValidateUserName(userNameDTO.Nombre_Usuario))
        //                throw new ArgumentException($"El nombre de usuario {userNameDTO.Nombre_Usuario} ya está en uso.");

        //            // find a user
        //            var user = await _usersCollection
        //                .Find(user => user.Nombre_Usuario == userName)
        //                .FirstOrDefaultAsync();

        //            // check if is not null
        //            if (user == null)
        //                throw new ArgumentException("Usuario no encontrado.");

        //            // changes
        //            user.Nombre_Usuario = userNameDTO.Nombre_Usuario;

        //            // modify
        //            await _usersCollection.ReplaceOneAsync(user => user.Nombre_Usuario == userName, user);

        //            // Retornar el modelo actualizado
        //            return user;
        //        }
        //        catch (MongoException ex)
        //        {
        //            // En caso de error
        //            throw new InvalidOperationException($"Error al modificar el nombre de usuario. {ex}");
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            // En caso de error
        //            throw new ArgumentException($"Error: {ex}");
        //        }
        //    }
        //    #endregion

        //    #region ModifyPassword
        //    public async Task<UsersModel> ModifyPassword(string userName, ModifyPassworDTO passwordDTO)
        //    {
        //        if (string.IsNullOrEmpty(userName)) // field verification
        //            throw new ArgumentException("El nombre usuario no puede estar vacío.", nameof(userName));
        //        if (!await ValidateUserName(userName)) // field verification
        //            throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
        //        if (string.IsNullOrEmpty(passwordDTO.Contraseña)) // field verification
        //            throw new ArgumentException("La contraseña no puede estar vacío.", nameof(passwordDTO.Contraseña));
        //        try
        //        {
        //            // find a user
        //            var user = await _usersCollection
        //                .Find(user => user.Nombre_Usuario == userName)
        //                .FirstOrDefaultAsync();

        //            // check if is not null
        //            if (user == null)
        //                throw new ArgumentException("Usuario no encontrado.");

        //            // changes
        //            user.Contraseña = BCrypt.Net.BCrypt.HashPassword(passwordDTO.Contraseña);

        //            // modify
        //            await _usersCollection.ReplaceOneAsync(user => user.Nombre_Usuario == userName, user);

        //            // user
        //            return user;
        //        }
        //        catch (MongoException ex)
        //        {
        //            // in case of error
        //            throw new InvalidOperationException($"Error al modificar el nombre de usuario. {ex}");
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            // in case of error
        //            throw new ArgumentException($"Error: {ex}");
        //        }
        //    }
        //    #endregion

        //    #region DeleteUser
        //    public async Task<UsersModel> DeleteUser(string userName)
        //    {
        //        if (string.IsNullOrEmpty(userName)) // field verification
        //            throw new ArgumentException("El ID de usuario no puede estar vacío.", nameof(userName));
        //        if (await ValidateUserName(userName) == false) // field verification
        //            throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
        //        try
        //        {
        //            // find a user
        //            var user = await _usersCollection
        //                .Find(user => user.Nombre_Usuario == userName)
        //                .FirstOrDefaultAsync();

        //            // check if is not null
        //            if (user == null)
        //                throw new ArgumentException("Usuario no encontrado.");

        //            // delete
        //            await _usersCollection.DeleteOneAsync(user => user.Nombre_Usuario == userName);

        //            // user
        //            return user;
        //        }
        //        catch (MongoException ex)
        //        {
        //            // in case of error
        //            throw new InvalidOperationException($"Error al modificar el nombre de usuario. {ex}");
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            // in case of error
        //            throw new ArgumentException($"Error: {ex}");
        //        }
        //    }
        //    #endregion
    }
}

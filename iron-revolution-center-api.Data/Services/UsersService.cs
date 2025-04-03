using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Client;
using iron_revolution_center_api.DTOs.User;
using iron_revolution_center_api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
        private readonly IConfiguration _config;
        private readonly IMongoCollection<UsersModel> _usersCollection;
        private readonly IMongoCollection<RegisterUserDTO> _registerUserCollection;
        private readonly IMongoCollection<RolesModel> _rolesCollection;
        private readonly IMongoCollection<EmployeesModel> _employeesCollection;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;

        // method to exclude _id field
        private static ProjectionDefinition<UsersModel> ExcludeIdProjection()
        {
            return Builders<UsersModel>.Projection.Exclude("_id");
        }

        private static ProjectionDefinition<RolesModel> ExcludeIdProjectionRole()
        {
            return Builders<RolesModel>.Projection.Exclude("_id");
        }

        private static ProjectionDefinition<EmployeesModel> ExcludeIdProjectionEmployees()
        {
            return Builders<EmployeesModel>.Projection.Exclude("_id");
        }

        private static ProjectionDefinition<ClientsModel> ExcludeIdProjectionClients()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }

        public UsersService(IMongoDatabase mongoDatabase, IConfiguration config)
        {
            _mongoDatabase = mongoDatabase;
            _config = config;
            _usersCollection = _mongoDatabase.GetCollection<UsersModel>("Users");
            _registerUserCollection = _mongoDatabase.GetCollection<RegisterUserDTO>("Users");
            _rolesCollection = _mongoDatabase.GetCollection<RolesModel>("Roles");
            _employeesCollection = _mongoDatabase.GetCollection<EmployeesModel>("Employees");
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
        }
        #endregion

        #region Validations
        private async Task<bool> IsUserRegistered(string userName)
        {
            try
            {
                // check user
                var UserNameIsAlreadyUsed = await _usersCollection
                    .CountDocumentsAsync(user => user.Nombre_Usuario == userName);

                // validate existence
                return UserNameIsAlreadyUsed > 0;
            } catch {
                // if not in used
                return false;
            }
        }

        private async Task<bool> IsRoleRegistered(string roleId)
        {
            try
            {
                // check user role
                var RoleExists = await _rolesCollection
                    .CountDocumentsAsync(rol => rol.Rol_Id == roleId);

                // validate existence
                return RoleExists > 0;
            } catch {
                // if not exist
                return false;
            }
        }
        #endregion

        #region JWT
        private string generateJwtToken(UsersModel user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Nombre_Usuario),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            if (!string.IsNullOrEmpty(user.NIP))
                claims.Add(new Claim("NIP", user.NIP));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<string> Login(Login login)
        {
            if (string.IsNullOrEmpty(login.Nombre_Usuario) || string.IsNullOrEmpty(login.Contraseña))
                throw new ArgumentException("El nombre de usuario y la contraseña no pueden estar vacíos.");
            try
            {
                var user = await _usersCollection
                .Find(user => user.Nombre_Usuario == login.Nombre_Usuario)
                .Project<UsersModel>(ExcludeIdProjection())
                .FirstOrDefaultAsync();

                if (user == null)
                    throw new ArgumentException($"Usuario '{login.Nombre_Usuario}' no encontrado.");

                if (!BCrypt.Net.BCrypt.Verify(login.Contraseña, user.Contraseña))
                    throw new ArgumentException("Contraseña incorrecta.");

                var jwt = generateJwtToken(user);

                return jwt;
            } catch (MongoException ex) {
                throw new InvalidOperationException($"Error al iniciar sesión. {ex.Message}");
            } catch (ArgumentException ex) {
                throw new ArgumentException($"Error: {ex.Message}");
            }
        }
        #endregion

        #region ListUsers
        public async Task<IEnumerable<UsersModel>> ListUsers()
        {
            try
            {
                // get users
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

        #region GetUsersByUserName
        public async Task<ShowUserDTO> GetUsersByUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.");
            if (!await IsUserRegistered(userName))
                throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
            try
            {
                // get user
                var user = await _usersCollection
                   .Find(user => user.Nombre_Usuario == userName)
                   .Project<UsersModel>(ExcludeIdProjection())
                   .FirstOrDefaultAsync();

                var userInfo = new ShowUserDTO
                {
                    Usuario = user
                };

                if (!string.IsNullOrEmpty(user.NIP))
                {
                    if (user.NIP.StartsWith("EM"))
                    {
                        var employee = await _employeesCollection
                            .Find(employee => employee.NIP == user.NIP)
                            .Project<EmployeesModel>(ExcludeIdProjectionEmployees())
                            .FirstOrDefaultAsync();

                        userInfo.Empleado = employee;
                    } else {
                        var client = await _clientsCollection
                            .Find(client => client.NIP == user.NIP)
                            .Project<ClientsModel>(ExcludeIdProjectionClients())
                            .FirstOrDefaultAsync();

                        userInfo.Cliente = client;
                    }
                }

                return userInfo;
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
        public async Task<ShowUserDTO> RegisterUser(RegisterUserDTO userDTO)
        {
            if (string.IsNullOrEmpty(userDTO.Nombre_Usuario))
                throw new ArgumentException($"El nombre de usuario no puede estar vacío.");
            if (await IsUserRegistered(userDTO.Nombre_Usuario))
                throw new ArgumentException($"Nombre de ususario: {userDTO.Nombre_Usuario} ya en uso");
            if (string.IsNullOrEmpty(userDTO.Rol)) 
                throw new ArgumentException($"El role no puede estar vacío.");
            if (!await IsRoleRegistered(userDTO.Rol))
                throw new ArgumentException($"El rol: {userDTO.Rol} no existe.");
            try
            {
                // get role 
                var rolName = await _rolesCollection
                    .Find(role => role.Rol_Id == userDTO.Rol)
                    .Project<RolesModel>(ExcludeIdProjectionRole())
                    .FirstOrDefaultAsync();

                userDTO.Rol = rolName.Nombre;

                if (!string.IsNullOrEmpty(userDTO.NIP))
                {
                    if (userDTO.NIP.StartsWith("EM"))
                    {
                        var employee = await _employeesCollection
                            .Find(employee => employee.NIP == userDTO.NIP)
                            .Project<EmployeesModel>(ExcludeIdProjectionEmployees())
                            .FirstOrDefaultAsync();

                        if (employee == null)
                            throw new ArgumentException($"NIP: {userDTO.NIP} no existe.");
                        else
                            userDTO.NIP = employee.NIP;
                    }
                    else
                    {
                        var client = await _clientsCollection
                            .Find(client => client.NIP == userDTO.NIP)
                            .Project<ClientsModel>(ExcludeIdProjectionClients())
                            .FirstOrDefaultAsync();

                        if (client == null)
                            throw new ArgumentException($"NIP: {userDTO.NIP} no existe.");
                        else
                            userDTO.NIP = client.NIP;
                    }
                }

                // register user
                var newUser = new RegisterUserDTO
                {
                    Nombre_Usuario = userDTO.Nombre_Usuario,
                    Contraseña = BCrypt.Net.BCrypt.HashPassword(userDTO.Contraseña),
                    Rol = rolName.Nombre,
                    NIP = userDTO.NIP
                };

                // check if is not null
                if (newUser == null)
                    throw new Exception($"Registro fallido.");

                // register
                await _registerUserCollection.InsertOneAsync(newUser);

                // user
                return await GetUsersByUserName(newUser.Nombre_Usuario);
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
        public async Task<ShowUserDTO> RegisterUserClient(RegisterUserDTO userDTO)
        {
            if (string.IsNullOrEmpty(userDTO.Nombre_Usuario))
                throw new ArgumentException($"El nombre de usuario no puede estar vacío.");
            if (await IsUserRegistered(userDTO.Nombre_Usuario)) 
                throw new ArgumentException($"Nombre de ususario: {userDTO.Nombre_Usuario} ya en uso");
            try
            {
                if (!string.IsNullOrEmpty(userDTO.NIP))
                {
                    var client = await _clientsCollection
                            .Find(client => client.NIP == userDTO.NIP)
                            .Project<ClientsModel>(ExcludeIdProjectionClients())
                            .FirstOrDefaultAsync();

                    if (client == null)
                        throw new ArgumentException($"NIP: {userDTO.NIP} no existe.");
                    else
                        userDTO.NIP = client.NIP;
                }

                // register user
                var newUser = new RegisterUserDTO
                {
                    Nombre_Usuario = userDTO.Nombre_Usuario,
                    Contraseña = BCrypt.Net.BCrypt.HashPassword(userDTO.Contraseña),
                    Rol = "Cliente",
                    NIP = userDTO.NIP
                };

                // check if is not null
                if (newUser == null)
                    throw new ArgumentException($"Registro fallido.");

                // register
                await _registerUserCollection.InsertOneAsync(newUser);

                // client
                return await GetUsersByUserName(newUser.Nombre_Usuario);
            } catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar usuario. {ex}");
            } catch (ArgumentException ex) {
                // in case of error 
                throw new ArgumentException($"Error: {ex}");
            }
        }
        #endregion

        #region ModifyUserName
        public async Task<ShowUserDTO> ModifyUserName(string userName, ModifyUserNameDTO userNameDTO)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException($"El nombre usuario no puede estar vacío.");
            if (!await IsUserRegistered(userName))
                throw new ArgumentException($"El nombre de usuario: {userNameDTO.Nombre_Usuario} no existe.");
            try
            {
                // check if user name is already used
                if (await IsUserRegistered(userNameDTO.Nombre_Usuario))
                    throw new ArgumentException($"El nombre de usuario {userNameDTO.Nombre_Usuario} ya está en uso.");

                // find a user
                var user = await _usersCollection
                    .Find(user => user.Nombre_Usuario == userName)
                    .Project<UsersModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // check if is not null
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado.");

                // changes
                user.Nombre_Usuario = userNameDTO.Nombre_Usuario;

                // modify
                await _usersCollection.ReplaceOneAsync(user => user.Nombre_Usuario == userName, user);

                return await GetUsersByUserName(userNameDTO.Nombre_Usuario);
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
        public async Task<ShowUserDTO> ModifyPassword(string userName, ModifyPassworDTO passwordDTO)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException($"El nombre usuario no puede estar vacío.");
            if (!await IsUserRegistered(userName))
                throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
            try
            {
                // find a user
                var user = await _usersCollection
                    .Find(user => user.Nombre_Usuario == userName)
                    .Project<UsersModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // check if is not null
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado.");

                // changes
                user.Contraseña = BCrypt.Net.BCrypt.HashPassword(passwordDTO.Contraseña);

                // modify
                await _usersCollection.ReplaceOneAsync(user => user.Nombre_Usuario == userName, user);

                // user
                return await GetUsersByUserName(user.Nombre_Usuario);
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
        public async Task<ShowUserDTO> DeleteUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.");
            if (!await IsUserRegistered(userName))
                throw new ArgumentException($"El nombre de usuario: {userName} no existe.");
            try
            {
                // find a user
                var user = await GetUsersByUserName(userName);

                // check if is not null
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado.");

                // delete
                await _usersCollection.DeleteOneAsync(user => user.Nombre_Usuario == userName);

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
    }
}

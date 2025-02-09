using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.DTOs.Users;
using iron_revolution_center_api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data.Service
{
    public class UsersService : iUsersService
    {
        #region MongoDB Configuration
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<UsersModel> _usersCollection;
        private readonly IMongoCollection<ClientsModel> _clientsCollection;

        // method to exclude _id field
        private static ProjectionDefinition<ClientsModel> ExcludeIdProjection()
        {
            return Builders<ClientsModel>.Projection.Exclude("_id");
        }

        public UsersService(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _usersCollection = _mongoDatabase.GetCollection<UsersModel>("Users");
            _clientsCollection = _mongoDatabase.GetCollection<ClientsModel>("Clients");
        }
        #endregion

        #region Vidations
        public async Task<bool> ValidateNIP(string NIP)
        {
            try
            {
                // filter 
                var filter = Builders<ClientsModel>.Filter.Eq(client => client.NIP, NIP);

                // count documents that match with the NIP
                var validationCount = await _clientsCollection.CountDocumentsAsync(filter);

                // validation
                return validationCount > 0;
            } catch {
                return false;
            }
        }

        public async Task<bool> ValidateUserName(string userName)
        {
            try
            {
                // filter
                var filter = Builders<UsersModel>.Filter.Eq(user => user.User_Name, userName);

                // count documents that match with the user_name
                var validationCount = await _usersCollection.CountDocumentsAsync(filter);

                // validation
                return validationCount > 0;
            } catch {
                return false;
            }
        }

        public async Task<bool> ValidateUserCredentials(string userName, string password)
        {
            try
            {
                // search for user
                var user = await _usersCollection
                    .Find(user => user.User_Name == userName)
                    .FirstOrDefaultAsync();
                if (user == null)
                    throw new Exception("Usuario no encontrado.");

                // check password
                bool userPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                if (userPasswordValid == false)
                    throw new Exception($"Contraseña incorrecta.");

                // validation
                return true;
            } catch (Exception ex) {
                throw new Exception($"Error: {ex.Message}");
            }
        }

        public async Task<bool> ValidateIsClientAssignedToUser(string userName, string NIP)
        {
            try
            {
                // filter
                var userFilter = Builders<UsersModel>.Filter.Eq(user => user.User_Name, userName) & // user
                                 Builders<UsersModel>.Filter.Eq(user => user.Client.NIP, NIP); // client

                // count documents that match with the user_name
                var validationCount = await _usersCollection.CountDocumentsAsync(userFilter);

                // validation
                return validationCount > 0;
            } catch {
                return false;
            }
        }
        #endregion

        #region GetUserByUserName
        public async Task<UsersModel> GetUserByUsername(string userName)
        {
            if (await ValidateUserName(userName) == false) // validate
                throw new InvalidOperationException($"Usuario {userName} no encontrado o credenciales incorrectas.");
            
            try
            {
                // user
                var user = await _usersCollection
                    .Find(user => user.User_Name == userName)
                    .FirstOrDefaultAsync();

                // client
                var client = await _clientsCollection
                    .Find(client => client.NIP == user.Client.NIP)
                    .FirstOrDefaultAsync();

                // return user info
                return new UsersModel
                {
                    User_Name = user.User_Name,
                    Password = user.Password,
                    Client = client
                };
            } catch (MongoException ex) {
                // in case of error, return
                throw new InvalidOperationException($"Error al obtener cliente. {ex.Message}");
            }
        }
        #endregion

        #region RegisterUser
        public async Task<RegisterUserDTOs> RegisterUser(RegisterUserDTOs userDTO)
        {
            if (userDTO == null) // field verification
                throw new ArgumentNullException(nameof(userDTO));
            if (await ValidateUserName(userDTO.User_Name)) // validate
                throw new InvalidOperationException($"El usuario {userDTO.User_Name} ya existe.");

            try
            {
                // get the collection
                var collection = _mongoDatabase.GetCollection<RegisterUserDTOs>("Users");

                // model of register user
                var newUser = new RegisterUserDTOs
                {
                    User_Name = userDTO.User_Name, // user name
                    Role = userDTO.Role,
                    Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password) // encrypted password
                };

                // insertion
                await collection.InsertOneAsync(newUser);

                // return user
                return newUser;
            }
            catch (MongoException ex) {
                // in case of error
                throw new InvalidOperationException($"Error al registrar usuario. {ex}");
            }
        }
        #endregion

        #region LoginUser
        public async Task<UsersModel> LoginUser(string userName, string password)
        {
            if (await ValidateUserCredentials(userName, password) == false) // validate
                throw new InvalidOperationException($"Usuario {userName} no encontrado o credenciales incorrectas.");

            try
            {
                // user
                var user = await _usersCollection
                    .Find(user => user.User_Name == userName)
                    .FirstOrDefaultAsync();
                if (user == null)
                    throw new InvalidOperationException($"Usuario {userName} no encontrado.");

                // client
                ClientsModel? client = null;
                if (user.Client != null)
                {
                    client = await _clientsCollection
                        .Find(client => client.NIP == user.Client.NIP)
                        .FirstOrDefaultAsync();
                }

                // return user info
                return new UsersModel
                {
                    User_Name = user.User_Name,
                    Password = user.Password,
                    Client = client
                };
            } catch (MongoException ex) {
                // in case of error, return
                throw new InvalidOperationException($"Error al iniciar sesión: {ex.Message}");
            }
        }
        #endregion

        #region AssignClient
        public async Task<UsersModel> AssignClient(string userName, string NIP, string password)
        {
            if (await ValidateUserCredentials(userName, password) == false) // validate
                throw new InvalidOperationException($"Usuario {userName} no encontrado o credenciales incorrectas.");

            if (await ValidateNIP(NIP) == false) // validate
                throw new InvalidOperationException($"Cliente no encontrado.");

            if (await ValidateIsClientAssignedToUser(userName, NIP) == true) // validate
                throw new InvalidOperationException($"El cliente con NIP {NIP} ya está asignado a otro usuario.");

            try
            {
                // client information
                var clientInfo = await _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .Project<ClientsModel>(ExcludeIdProjection())
                    .FirstOrDefaultAsync();

                // assign client to user
                var assignResult = await _usersCollection.UpdateOneAsync(
                    Builders<UsersModel>.Filter.Eq(user => user.User_Name, userName), // find client
                    Builders<UsersModel>.Update.Set(user => user.Client.NIP, clientInfo.NIP)); // assign NIP
                if (assignResult.ModifiedCount == 0)
                    throw new Exception("No se pudo actualizar el usuario.");

                // user
                var userTask = _usersCollection
                    .Find(user => user.User_Name == userName)
                    .FirstOrDefaultAsync();

                // client 
                var clientTask = _clientsCollection
                    .Find(client => client.NIP == NIP)
                    .FirstOrDefaultAsync();

                // execute at the same time
                await Task.WhenAll(userTask, clientTask);

                // result of the consultation
                var user = userTask.Result;
                var client = clientTask.Result;

                // verification
                if (user == null || client == null)
                    throw new ArgumentException("Error al mostrar la infomación de usuario");

                // return user info
                return new UsersModel
                {
                    User_Name = user.User_Name,
                    Password = user.Password,
                    Client = client
                };
            } catch (MongoException ex) {
                // in case of error, return
                throw new InvalidOperationException($"Error al asignar el cliente: {ex.Message}");
            }
        }
        #endregion
    }
}

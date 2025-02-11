using iron_revolution_center_api.Data.Interface;
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
    }
}

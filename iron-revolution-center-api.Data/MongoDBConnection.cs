using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Data
{
    // mongodb connection
    public class MongoDBConnection
    {
        public string _connection;
        public MongoDBConnection(string connection) => this._connection = connection;
    }
}

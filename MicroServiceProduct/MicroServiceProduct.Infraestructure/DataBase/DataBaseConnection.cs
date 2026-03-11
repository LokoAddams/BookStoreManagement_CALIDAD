using MicroServiceProduct.Domain.Services;
using Npgsql;
using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceProduct.Infraestructure.DataBase
{
    public class DataBaseConnection : IDataBase
    {
        private static DataBaseConnection? _instance;
        private static readonly object _padlock = new object();
        private readonly string _connectionString;

        private DataBaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static DataBaseConnection GetInstance(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString must be provided", nameof(connectionString));

            if (_instance == null)
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new DataBaseConnection(connectionString);
                    }
                }
            }
            return _instance;
        }

        public DbConnection GetConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}

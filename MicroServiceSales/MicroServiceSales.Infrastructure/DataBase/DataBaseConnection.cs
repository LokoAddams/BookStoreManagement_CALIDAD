using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroServiceSales.Domain.Interfaces;

namespace MicroServiceSales.Infrastructure.DataBase
{
    public class DataBaseConnection : IDataBase
    {
        private static DataBaseConnection? _instance;
        private static readonly object _padlock = new object();
        private readonly string _connectionString;
        private readonly Func<string, NpgsqlConnection> _connectionFactory;
        private readonly Action<NpgsqlConnection> _openConnection;

        private DataBaseConnection(string connectionString)
            : this(connectionString, null, null)
        {
        }

        internal DataBaseConnection(
            string connectionString,
            Func<string, NpgsqlConnection>? connectionFactory,
            Action<NpgsqlConnection>? openConnection)
        {
            _connectionString = connectionString;
            _connectionFactory = connectionFactory ?? (cs => new NpgsqlConnection(cs));
            _openConnection = openConnection ?? (conn => conn.Open());
        }

        public static DataBaseConnection GetInstance(string connectionString)
        {
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

        public NpgsqlConnection GetConnection()
        {
            var conn = _connectionFactory(_connectionString);
            _openConnection(conn);
            return conn;
        }
    }
}

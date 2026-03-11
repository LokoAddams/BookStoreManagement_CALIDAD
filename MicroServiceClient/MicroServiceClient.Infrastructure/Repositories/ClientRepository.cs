using MicroServiceClient.Domain.Interfaces;
using MicroServiceClient.Domain.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceClient.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly IDataBase _database;

        public ClientRepository(IDataBase database)
        {
            _database = database;
        }

        public List<Client> GetAll()
        {
            var clients = new List<Client>();
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM clients WHERE is_active = TRUE ORDER BY last_name, first_name";
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                clients.Add(MapClient(reader));
            }

            return clients;
        }

        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            await using var conn = (NpgsqlConnection)_database.GetConnection();
            await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM clients WHERE is_active = TRUE", conn);
            var count = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(count);
        }

        public async Task<PagedResult<Client>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var clients = new List<Client>();
            int offset = (page - 1) * pageSize;

            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM clients 
                                WHERE is_active = TRUE 
                                ORDER BY last_name, first_name 
                                LIMIT @limit OFFSET @offset";

            var paramLimit = cmd.CreateParameter();
            paramLimit.ParameterName = "@limit";
            paramLimit.Value = pageSize;
            cmd.Parameters.Add(paramLimit);

            var paramOffset = cmd.CreateParameter();
            paramOffset.ParameterName = "@offset";
            paramOffset.Value = offset;
            cmd.Parameters.Add(paramOffset);

            using var reader = cmd.ExecuteReader();

            int colId = reader.GetOrdinal("id");
            int colCi = reader.GetOrdinal("ci");
            int colFirstName = reader.GetOrdinal("first_name");
            int colLastName = reader.GetOrdinal("last_name");
            int colEmail = reader.GetOrdinal("email");
            int colPhone = reader.GetOrdinal("phone");
            int colAddress = reader.GetOrdinal("address");
            int colCreatedAt = reader.GetOrdinal("created_at");

            while (await ((NpgsqlDataReader)reader).ReadAsync(ct))
            {
                clients.Add(new Client
                {
                    Id = reader.GetGuid(colId),
                    Ci = reader.IsDBNull(colCi) ? string.Empty : reader.GetString(colCi),
                    FirstName = reader.GetString(colFirstName),
                    LastName = reader.GetString(colLastName),
                    Email = reader.IsDBNull(colEmail) ? null : reader.GetString(colEmail),
                    Phone = reader.IsDBNull(colPhone) ? null : reader.GetString(colPhone),
                    Address = reader.IsDBNull(colAddress) ? null : reader.GetString(colAddress),
                    CreatedAt = reader.GetDateTime(colCreatedAt)
                });
            }

            return new PagedResult<Client>
            {
                Items = clients,
                Page = page,
                PageSize = pageSize,
                TotalItems = await CountAsync(ct)
            };
        }

        public Client? Read(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM clients WHERE id = @id";

            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = id;
            cmd.Parameters.Add(paramId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapClient(reader);
            }

            return null;
        }

        public void Create(Client client)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO clients (ci, first_name, last_name, email, phone, address)
                VALUES (@ci, @first_name, @last_name, @email, @phone, @address)";

            var paramCi = cmd.CreateParameter();
            paramCi.ParameterName = "@ci";
            paramCi.Value = client.Ci;
            cmd.Parameters.Add(paramCi);

            var paramFirstName = cmd.CreateParameter();
            paramFirstName.ParameterName = "@first_name";
            paramFirstName.Value = client.FirstName;
            cmd.Parameters.Add(paramFirstName);

            var paramLastName = cmd.CreateParameter();
            paramLastName.ParameterName = "@last_name";
            paramLastName.Value = client.LastName;
            cmd.Parameters.Add(paramLastName);

            var paramEmail = cmd.CreateParameter();
            paramEmail.ParameterName = "@email";
            paramEmail.Value = client.Email ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramEmail);

            var paramPhone = cmd.CreateParameter();
            paramPhone.ParameterName = "@phone";
            paramPhone.Value = client.Phone ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramPhone);

            var paramAddress = cmd.CreateParameter();
            paramAddress.ParameterName = "@address";
            paramAddress.Value = client.Address ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramAddress);

            cmd.ExecuteNonQuery();
        }

        public void Update(Client client)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE clients SET 
                    ci = @ci,
                    first_name = @first_name,
                    last_name = @last_name,
                    email = @email,
                    phone = @phone,
                    address = @address
                WHERE id = @id";

            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = client.Id;
            cmd.Parameters.Add(paramId);

            var paramCi = cmd.CreateParameter();
            paramCi.ParameterName = "@ci";
            paramCi.Value = client.Ci;
            cmd.Parameters.Add(paramCi);

            var paramFirstName = cmd.CreateParameter();
            paramFirstName.ParameterName = "@first_name";
            paramFirstName.Value = client.FirstName;
            cmd.Parameters.Add(paramFirstName);

            var paramLastName = cmd.CreateParameter();
            paramLastName.ParameterName = "@last_name";
            paramLastName.Value = client.LastName;
            cmd.Parameters.Add(paramLastName);

            var paramEmail = cmd.CreateParameter();
            paramEmail.ParameterName = "@email";
            paramEmail.Value = client.Email ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramEmail);

            var paramPhone = cmd.CreateParameter();
            paramPhone.ParameterName = "@phone";
            paramPhone.Value = client.Phone ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramPhone);

            var paramAddress = cmd.CreateParameter();
            paramAddress.ParameterName = "@address";
            paramAddress.Value = client.Address ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramAddress);

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE clients SET is_active = FALSE WHERE id = @id";

            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = id;
            cmd.Parameters.Add(paramId);

            cmd.ExecuteNonQuery();
        }

        public async Task<Client?> GetByCiAsync(string ci, CancellationToken ct = default)
        {
            await using var conn = (NpgsqlConnection)_database.GetConnection();
            await using var cmd = new NpgsqlCommand("SELECT * FROM clients WHERE ci = @ci AND is_active = TRUE LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@ci", ci);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return MapClient(reader);
            }

            return null;
        }

        public bool ExistsByCi(string ci, Guid? excludeId = null)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            if (excludeId.HasValue)
            {
                cmd.CommandText = "SELECT 1 FROM clients WHERE ci = @ci AND id <> @id AND is_active = TRUE LIMIT 1";
                var pId = cmd.CreateParameter();
                pId.ParameterName = "@id";
                pId.Value = excludeId.Value;
                cmd.Parameters.Add(pId);
            }
            else
            {
                cmd.CommandText = "SELECT 1 FROM clients WHERE ci = @ci AND is_active = TRUE LIMIT 1";
            }

            var pCi = cmd.CreateParameter();
            pCi.ParameterName = "@ci";
            pCi.Value = ci;
            cmd.Parameters.Add(pCi);

            using var reader = cmd.ExecuteReader();
            return reader.Read();
        }

        private static Client MapClient(System.Data.IDataReader reader)
        {
            return new Client
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Ci = reader.IsDBNull(reader.GetOrdinal("ci")) ? string.Empty : reader.GetString(reader.GetOrdinal("ci")),
                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            };
        }
    }
}

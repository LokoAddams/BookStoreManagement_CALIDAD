using Npgsql;
using MicroServiceUsers.Domain.Interfaces;
using MicroServiceDistributors.Domain.Models;
using MicroServiceDistributors.Domain.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading;

namespace ServiceDistributors.Infrastructure.Repositories
{
    public class DistributorRepository : IDistributorRepository
    {
        private readonly IDataBase _database;

        public DistributorRepository(IDataBase database)
        {
            _database = database;
        }

        public void Create(Distributor distributor)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO distributors (name, contact_email, phone, address)
                VALUES (@name, @contact_email, @phone, @address)";

            var paramName = cmd.CreateParameter();
            paramName.ParameterName = "@name";
            paramName.Value = distributor.Name;
            cmd.Parameters.Add(paramName);

            var paramEmail = cmd.CreateParameter();
            paramEmail.ParameterName = "@contact_email";
            paramEmail.Value = distributor.ContactEmail ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramEmail);

            var paramPhone = cmd.CreateParameter();
            paramPhone.ParameterName = "@phone";
            paramPhone.Value = distributor.Phone ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramPhone);

            var paramAddress = cmd.CreateParameter();
            paramAddress.ParameterName = "@address";
            paramAddress.Value = distributor.Address ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramAddress);

            cmd.ExecuteNonQuery();
        }

        public Distributor? Read(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM distributors WHERE id = @id order by name";

            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = id;
            cmd.Parameters.Add(paramId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Distributor
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ContactEmail = reader.IsDBNull(reader.GetOrdinal("contact_email")) ? null : reader.GetString(reader.GetOrdinal("contact_email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };
            }

            return null;
        }

        public void Update(Distributor distributor)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE distributors SET 
                    name = @name,
                    contact_email = @contact_email,
                    phone = @phone,
                    address = @address
                WHERE id = @id";
            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = distributor.Id;
            cmd.Parameters.Add(paramId);

            var paramName = cmd.CreateParameter();
            paramName.ParameterName = "@name";
            paramName.Value = distributor.Name;
            cmd.Parameters.Add(paramName);

            var paramEmail = cmd.CreateParameter();
            paramEmail.ParameterName = "@contact_email";
            paramEmail.Value = distributor.ContactEmail ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramEmail);

            var paramPhone = cmd.CreateParameter();
            paramPhone.ParameterName = "@phone";
            paramPhone.Value = distributor.Phone ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramPhone);

            var paramAddress = cmd.CreateParameter();
            paramAddress.ParameterName = "@address";
            paramAddress.Value = distributor.Address ?? (object)DBNull.Value;
            cmd.Parameters.Add(paramAddress);

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE distributors SET is_active = FALSE WHERE id = @id";

            var paramId = cmd.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = id;
            cmd.Parameters.Add(paramId);

            cmd.ExecuteNonQuery();
        }

        public List<Distributor> GetAll()
        {
            var distributors = new List<Distributor>();
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM distributors WHERE is_active = TRUE ORDER BY name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                distributors.Add(new Distributor
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ContactEmail = reader.IsDBNull(reader.GetOrdinal("contact_email")) ? null : reader.GetString(reader.GetOrdinal("contact_email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                });
            }
            return distributors;
        }
        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            await using var conn = (NpgsqlConnection)_database.GetConnection();
            await using var cmd = new NpgsqlCommand(@"SELECT COUNT(*) 
                                                      FROM distributors p 
                                                      WHERE p.is_active = TRUE", conn);
            var count = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(count);
        }

        public async Task<PagedResult<Distributor>> GetPageAsync(int page, int pageSize, CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;


            using var connection = _database.GetConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"select * 
                                from distributors where is_active = true 
                                order by name 
                                limit @limit offset @offset";

            int limit = pageSize;
            int offset = (page - 1) * pageSize;

            var paramLimit = cmd.CreateParameter();
            paramLimit.ParameterName = "@limit";
            paramLimit.Value = limit;
            cmd.Parameters.Add(paramLimit);

            var paramOffset = cmd.CreateParameter();
            paramOffset.ParameterName = "@offset";
            paramOffset.Value = offset;
            cmd.Parameters.Add(paramOffset);


            using var reader = cmd.ExecuteReader();

            int colum_id_index = reader.GetOrdinal("id");
            int colum_name_index = reader.GetOrdinal("name");
            int colum_contacEmail_index = reader.GetOrdinal("contact_email");
            int colum_contacPhone_index = reader.GetOrdinal("phone");
            int colum_addres_index = reader.GetOrdinal("address");
            int colum_createAt_index = reader.GetOrdinal("created_at");


            var distributors = new List<Distributor>();
            while (await reader.ReadAsync())
            {
                distributors.Add(new Distributor
                {
                    Id = reader.GetGuid(colum_id_index),
                    Name = reader.GetString(colum_name_index),
                    ContactEmail = reader.IsDBNull(colum_contacEmail_index) ? null : reader.GetString(colum_contacEmail_index),
                    Phone = reader.IsDBNull(colum_contacPhone_index) ? null : reader.GetString(colum_contacPhone_index),
                    Address = reader.IsDBNull(colum_addres_index) ? null : reader.GetString(colum_addres_index),
                    CreatedAt = reader.GetDateTime(colum_createAt_index)
                });
            }


            return new PagedResult<Distributor>
            {
                Items = distributors,
                Page = page,
                PageSize = pageSize,
                TotalItems = await CountAsync(ct)
            };
        }
    }
}

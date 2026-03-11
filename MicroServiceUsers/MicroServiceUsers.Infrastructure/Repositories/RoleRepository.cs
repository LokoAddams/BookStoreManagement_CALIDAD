using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceUsers.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IDataBase _database;

        public RoleRepository(IDataBase database)
        {
            _database = database;
        }

        public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
        {
            var roles = new List<Role>();
            var sql = "SELECT id, name, description FROM roles ORDER BY name";

            await using var conn = _database.GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                roles.Add(new Role
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }

            return roles;
        }

        public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var sql = "SELECT id, name, description FROM roles WHERE id = @id";

            await using var conn = _database.GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new Role
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                };
            }

            return null;
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            var sql = "SELECT id, name, description FROM roles WHERE LOWER(name) = LOWER(@name)";

            await using var conn = _database.GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", name);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new Role
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                };
            }

            return null;
        }

        public async Task CreateAsync(Role role, CancellationToken ct = default)
        {
            var sql = @"INSERT INTO roles (id, name, description) 
                        VALUES (@id, @name, @description)";

            await using var conn = _database.GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);

            role.Id = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", role.Id);
            cmd.Parameters.AddWithValue("@name", role.Name);
            cmd.Parameters.AddWithValue("@description", (object?)role.Description ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}

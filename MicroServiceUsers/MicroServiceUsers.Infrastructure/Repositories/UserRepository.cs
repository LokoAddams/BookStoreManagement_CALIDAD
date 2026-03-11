using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceUsers.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDataBase _database;

        public UserRepository(IDataBase database)
        {
            _database = database;
        }

        public async Task<User?> GetByUserOrEmailAsync(string userOrEmail, CancellationToken ct = default)
        {
            var input = (userOrEmail ?? string.Empty).Trim().ToLowerInvariant();

            var sql = @"SELECT id, username, email, first_name, last_name, middle_name, 
                               password_hash, is_active, must_change_password 
                        FROM users 
                        WHERE (LOWER(username)=@ue OR LOWER(email)=@ue) 
                        LIMIT 1";

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            
            var param = cmd.CreateParameter();
            param.ParameterName = "@ue";
            param.Value = input;
            cmd.Parameters.Add(param);

            await using var reader = await ((NpgsqlCommand)cmd).ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
        {
            var roles = new List<string>();
            var sql = @"SELECT r.name 
                        FROM roles r 
                        JOIN user_roles ur ON ur.role_id = r.id 
                        WHERE ur.user_id = @id";

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            
            var param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.Value = userId;
            cmd.Parameters.Add(param);

            await using var reader = await ((NpgsqlCommand)cmd).ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                roles.Add(reader.GetString(0));

            return roles;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var sql = @"SELECT id, username, email, first_name, last_name, middle_name, 
                               password_hash, is_active, must_change_password 
                        FROM users WHERE id = @id";

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            
            var param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.Value = id;
            cmd.Parameters.Add(param);

            await using var reader = await ((NpgsqlCommand)cmd).ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
        {
            var users = new List<User>();

            var sql = @"SELECT id, username, email, first_name, last_name, middle_name, 
                               password_hash, is_active, must_change_password 
                        FROM users 
                        WHERE is_active = TRUE 
                        ORDER BY username";

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            
            await using var reader = await ((NpgsqlCommand)cmd).ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM users WHERE is_active = TRUE";
            var count = await ((NpgsqlCommand)cmd).ExecuteScalarAsync(ct);
            return Convert.ToInt32(count);
        }

        public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var users = new List<User>();
            int offset = (page - 1) * pageSize;

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, username, email, first_name, last_name, middle_name, 
                                       password_hash, is_active, must_change_password 
                                FROM users 
                                WHERE is_active = TRUE 
                                ORDER BY username
                                LIMIT @limit OFFSET @offset";

            var paramLimit = cmd.CreateParameter();
            paramLimit.ParameterName = "@limit";
            paramLimit.Value = pageSize;
            cmd.Parameters.Add(paramLimit);

            var paramOffset = cmd.CreateParameter();
            paramOffset.ParameterName = "@offset";
            paramOffset.Value = offset;
            cmd.Parameters.Add(paramOffset);

            await using var reader = await ((NpgsqlCommand)cmd).ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                users.Add(MapUser(reader));
            }

            return new PagedResult<User>
            {
                Items = users,
                Page = page,
                PageSize = pageSize,
                TotalItems = await CountAsync(ct)
            };
        }

        public async Task CreateAsync(User user, string password, List<string> roles, CancellationToken ct = default)
        {
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, password);

            var sql = @"INSERT INTO users 
                        (id, username, email, first_name, last_name, middle_name, 
                         password_hash, is_active, must_change_password)
                        VALUES 
                        (@id, @username, @email, @firstName, @lastName, @middleName, 
                         @passwordHash, @isActive, @mustChange)";

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var newId = Guid.NewGuid();
            user.Id = newId;

            AddParameter(cmd, "@id", newId);
            AddParameter(cmd, "@username", user.Username);
            AddParameter(cmd, "@email", user.Email);
            AddParameter(cmd, "@firstName", user.FirstName ?? string.Empty);
            AddParameter(cmd, "@lastName", user.LastName ?? string.Empty);
            AddParameter(cmd, "@middleName", (object?)user.MiddleName ?? DBNull.Value);
            AddParameter(cmd, "@passwordHash", user.PasswordHash);
            AddParameter(cmd, "@isActive", user.IsActive);
            AddParameter(cmd, "@mustChange", user.MustChangePassword);

            await ((NpgsqlCommand)cmd).ExecuteNonQueryAsync(ct);

            foreach (var roleName in roles)
            {
                await using var roleCmd = conn.CreateCommand();
                roleCmd.CommandText = @"INSERT INTO user_roles (user_id, role_id) 
                                        SELECT @userId, r.id FROM roles r WHERE r.name = @roleName";
                AddParameter(roleCmd, "@userId", newId);
                AddParameter(roleCmd, "@roleName", roleName);
                await ((NpgsqlCommand)roleCmd).ExecuteNonQueryAsync(ct);
            }
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            var sql = @"UPDATE users 
                        SET username = @username, 
                            email = @email, 
                            first_name = @firstName, 
                            last_name = @lastName, 
                            middle_name = @middleName, 
                            password_hash = @passwordHash, 
                            is_active = @isActive, 
                            must_change_password = @mustChange
                        WHERE id = @id";

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParameter(cmd, "@id", user.Id);
            AddParameter(cmd, "@username", user.Username);
            AddParameter(cmd, "@email", user.Email);
            AddParameter(cmd, "@firstName", user.FirstName ?? string.Empty);
            AddParameter(cmd, "@lastName", user.LastName ?? string.Empty);
            AddParameter(cmd, "@middleName", (object?)user.MiddleName ?? DBNull.Value);
            AddParameter(cmd, "@passwordHash", user.PasswordHash);
            AddParameter(cmd, "@isActive", user.IsActive);
            AddParameter(cmd, "@mustChange", user.MustChangePassword);

            await ((NpgsqlCommand)cmd).ExecuteNonQueryAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET is_active = FALSE WHERE id = @id";
            AddParameter(cmd, "@id", id);
            await ((NpgsqlCommand)cmd).ExecuteNonQueryAsync(ct);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            var user = await GetByIdAsync(userId, ct);
            if (user is null || !user.IsActive)
                return false;

            var hasher = new PasswordHasher<User>();
            var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            
            if (verify == PasswordVerificationResult.Failed)
                return false;

            var newPasswordHash = hasher.HashPassword(user, newPassword);

            await using var conn = _database.GetConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE users 
                                SET password_hash = @passwordHash, 
                                    must_change_password = FALSE
                                WHERE id = @id";
            AddParameter(cmd, "@id", userId);
            AddParameter(cmd, "@passwordHash", newPasswordHash);

            var rowsAffected = await ((NpgsqlCommand)cmd).ExecuteNonQueryAsync(ct);
            return rowsAffected > 0;
        }

        private static User MapUser(NpgsqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                FirstName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                LastName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                MiddleName = reader.IsDBNull(5) ? null : reader.GetString(5),
                PasswordHash = reader.GetString(6),
                IsActive = reader.GetBoolean(7),
                MustChangePassword = reader.IsDBNull(8) ? false : reader.GetBoolean(8)
            };
        }

        private static void AddParameter(System.Data.IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            cmd.Parameters.Add(param);
        }
    }
}

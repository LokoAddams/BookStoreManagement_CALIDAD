using System;
using System.Collections.Generic;
using System.Data.Common;
using MicroServiceProduct.Domain.Models;
using MicroServiceProduct.Domain.Services;
using MicroServiceProduct.Domain.Interfaces;

namespace MicroServiceProduct.Infraestructure.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDataBase _database;

        public CategoryRepository(IDataBase database)
        {
            _database = database;
        }

        public void Create(Category entity)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO categories (id, name, description, created_at, is_active) VALUES (@id, @name, @description, @created_at, @is_active)";

            var pId = cmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id; cmd.Parameters.Add(pId);
            var pName = cmd.CreateParameter(); pName.ParameterName = "@name"; pName.Value = entity.Name; cmd.Parameters.Add(pName);
            var pDesc = cmd.CreateParameter(); pDesc.ParameterName = "@description"; pDesc.Value = (object?)entity.Description ?? DBNull.Value; cmd.Parameters.Add(pDesc);
            var pCreated = cmd.CreateParameter(); pCreated.ParameterName = "@created_at"; pCreated.Value = entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt; cmd.Parameters.Add(pCreated);
            var pActive = cmd.CreateParameter(); pActive.ParameterName = "@is_active"; pActive.Value = true; cmd.Parameters.Add(pActive);

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE categories SET is_active = FALSE WHERE id = @id";
            var pId = cmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = id; cmd.Parameters.Add(pId);
            cmd.ExecuteNonQuery();
        }

        public Category? Read(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, name, description FROM categories WHERE id = @id";
            var p = cmd.CreateParameter(); p.ParameterName = "@id"; p.Value = id; cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Category
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                };
            }

            return null;
        }

        public void Update(Category entity)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE categories SET name = @name, description = @description WHERE id = @id";

            var pId = cmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = entity.Id; cmd.Parameters.Add(pId);
            var pName = cmd.CreateParameter(); pName.ParameterName = "@name"; pName.Value = entity.Name; cmd.Parameters.Add(pName);
            var pDesc = cmd.CreateParameter(); pDesc.ParameterName = "@description"; pDesc.Value = (object?)entity.Description ?? DBNull.Value; cmd.Parameters.Add(pDesc);

            cmd.ExecuteNonQuery();
        }

        public List<Category> GetAll()
        {
            var categories = new List<Category>();
            using var conn = _database.GetConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, name FROM categories WHERE is_active = TRUE";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            return categories;
        }
    }
}

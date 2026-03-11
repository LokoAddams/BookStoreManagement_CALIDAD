using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Domain.Models;
using MicroServiceProduct.Domain.Services;
using Npgsql;
using ServiceCommon.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceProduct.Infraestructure.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDataBase _database;

        public ProductRepository(IDataBase database)
        {
            _database = database;
        }

        public void Create(Product product)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO products (id, name, description, category_id, category_name, price, stock, created_at, is_active)
                                VALUES (@id, @name, @description, @category_id,
                                        (SELECT name FROM categories WHERE id = @category_id),
                                        @price, @stock, @created_at, TRUE)";

            var pId = cmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = product.Id; cmd.Parameters.Add(pId);
            var pName = cmd.CreateParameter(); pName.ParameterName = "@name"; pName.Value = product.Name; cmd.Parameters.Add(pName);
            var pDesc = cmd.CreateParameter(); pDesc.ParameterName = "@description"; pDesc.Value = (object?)product.Description ?? DBNull.Value; cmd.Parameters.Add(pDesc);
            var pCat = cmd.CreateParameter(); pCat.ParameterName = "@category_id"; pCat.Value = product.CategoryId != Guid.Empty ? (object)product.CategoryId : DBNull.Value; cmd.Parameters.Add(pCat);
            var pPrice = cmd.CreateParameter(); pPrice.ParameterName = "@price"; pPrice.Value = product.Price; cmd.Parameters.Add(pPrice);
            var pStock = cmd.CreateParameter(); pStock.ParameterName = "@stock"; pStock.Value = product.Stock; cmd.Parameters.Add(pStock);
            var pCreated = cmd.CreateParameter(); pCreated.ParameterName = "@created_at"; pCreated.Value = product.CreatedAt == default ? DateTime.UtcNow : product.CreatedAt; cmd.Parameters.Add(pCreated);

            cmd.ExecuteNonQuery();
        }

        public Product? Read(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT p.id, p.name, p.description, p.category_id,
                                       COALESCE(c.name, p.category_name) AS category_name,
                                       p.price, p.stock, p.created_at
                                FROM products p
                                LEFT JOIN categories c ON p.category_id = c.id
                                WHERE p.id = @id";
            var pid = cmd.CreateParameter(); pid.ParameterName = "@id"; pid.Value = id; cmd.Parameters.Add(pid);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Product
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString(reader.GetOrdinal("description")),
                    CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("category_id")),
                    CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? null : reader.GetString(reader.GetOrdinal("category_name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };
            }

            return null;
        }

        public void Update(Product product)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE products p
                                SET name = @name,
                                    description = @description,
                                    category_id = @category_id,
                                    category_name = COALESCE(c.name, @category_name, p.category_name),
                                    price = @price,
                                    stock = @stock
                                FROM categories c
                                WHERE p.id = @id AND c.id = @category_id";

            var pId = cmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = product.Id; cmd.Parameters.Add(pId);
            var pName = cmd.CreateParameter(); pName.ParameterName = "@name"; pName.Value = product.Name; cmd.Parameters.Add(pName);
            var pDesc = cmd.CreateParameter(); pDesc.ParameterName = "@description"; pDesc.Value = (object?)product.Description ?? DBNull.Value; cmd.Parameters.Add(pDesc);
            var pCat = cmd.CreateParameter(); pCat.ParameterName = "@category_id"; pCat.Value = product.CategoryId != Guid.Empty ? (object)product.CategoryId : DBNull.Value; cmd.Parameters.Add(pCat);
            var pCatName = cmd.CreateParameter(); pCatName.ParameterName = "@category_name"; pCatName.Value = (object?)product.CategoryName ?? DBNull.Value; cmd.Parameters.Add(pCatName);
            var pPrice = cmd.CreateParameter(); pPrice.ParameterName = "@price"; pPrice.Value = product.Price; cmd.Parameters.Add(pPrice);
            var pStock = cmd.CreateParameter(); pStock.ParameterName = "@stock"; pStock.Value = product.Stock; cmd.Parameters.Add(pStock);

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE products SET is_active = FALSE WHERE id = @id";
            var pId = cmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = id; cmd.Parameters.Add(pId);
            cmd.ExecuteNonQuery();
        }

        public List<Product> GetAll()
        {
            var products = new List<Product>();
            using var conn = _database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT p.id, p.name, p.description, p.category_id,
                                       COALESCE(c.name, p.category_name) AS category_name,
                                       p.price, p.stock, p.created_at
                                FROM products p
                                LEFT JOIN categories c ON p.category_id = c.id
                                WHERE p.is_active = TRUE
                                ORDER BY p.name";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString(reader.GetOrdinal("description")),
                    CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("category_id")),
                    CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? null : reader.GetString(reader.GetOrdinal("category_name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                });
            }
            return products;
        }

        public bool TryReserveStock(Dictionary<Guid, int> items, out string? error)
        {
            error = null;
            using var conn = _database.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                foreach (var kv in items)
                {
                    var productId = kv.Key;
                    var qty = kv.Value;

                    using var check = conn.CreateCommand();
                    check.Transaction = tx;
                    check.CommandText = "SELECT name, stock FROM products WHERE id = @id FOR UPDATE";
                    var pId = check.CreateParameter(); pId.ParameterName = "@id"; pId.Value = productId; check.Parameters.Add(pId);

                    using var reader = check.ExecuteReader();
                    if (!reader.Read())
                    {
                        error = $"Producto no encontrado (ID: {productId})";
                        reader.Close();
                        tx.Rollback();
                        return false;
                    }

                    var productName = reader.GetString(0);
                    var currentStock = reader.GetInt32(1);
                    reader.Close();

                    if (qty <= 0)
                    {
                        error = $"Cantidad inválida ({qty}) para el producto '{productName}'";
                        tx.Rollback();
                        return false;
                    }
                    if (currentStock < qty)
                    {
                        error = $"Stock insuficiente para '{productName}'. Solicitado: {qty}, Disponible: {currentStock}";
                        tx.Rollback();
                        return false;
                    }

                    using var update = conn.CreateCommand();
                    update.Transaction = tx;
                    update.CommandText = "UPDATE products SET stock = stock - @qty WHERE id = @id";
                    var uId = update.CreateParameter(); uId.ParameterName = "@id"; uId.Value = productId; update.Parameters.Add(uId);
                    var uQty = update.CreateParameter(); uQty.ParameterName = "@qty"; uQty.Value = qty; update.Parameters.Add(uQty);
                    update.ExecuteNonQuery();
                }

                tx.Commit();
                return true;
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                error = ex.Message;
                return false;
            }
        }
        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            await using var conn = (NpgsqlConnection)_database.GetConnection();
            await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM products p WHERE p.is_active = TRUE", conn);
            var count = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(count);
        }

        public async Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = new PagedResult<Product>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = await CountAsync(ct)
            };

            await using var conn = (NpgsqlConnection)_database.GetConnection();

            var offset = (page - 1) * pageSize;
            var sql = @"SELECT p.id, p.name, p.description, p.category_id,
                               COALESCE(c.name, p.category_name) AS category_name,
                               p.price, p.stock, p.created_at
                        FROM products p
                        LEFT JOIN categories c ON p.category_id = c.id
                        WHERE p.is_active = TRUE
                        ORDER BY p.name
                        LIMIT @limit OFFSET @offset";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@limit", pageSize);
            cmd.Parameters.AddWithValue("@offset", offset);

            var items = new List<Product>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                items.Add(new Product
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                    CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("category_id")),
                    CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? null : reader.GetString(reader.GetOrdinal("category_name")),
                    Price = reader.GetDecimal(reader.GetOrdinal("price")),
                    Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                });
            }

            result.Items = items;
            return result;
        }
    }
}
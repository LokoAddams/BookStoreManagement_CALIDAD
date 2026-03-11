using System;
using System.Collections.Generic;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using Npgsql;
using NpgsqlTypes;

namespace MicroServiceSales.Infrastructure.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly IDataBase _database;

        public SalesRepository(IDataBase database)
        {
            _database = database;
        }

        public List<Sale> GetAll()
        {
            var sales = new List<Sale>();
            using var conn = _database.GetConnection();
            using var cmd = new NpgsqlCommand(@"
                SELECT id, client_id, user_id, sale_date, subtotal, total, status,
                       cancellation_reason, cancelled_at, cancelled_by, created_at
                FROM sales
                ORDER BY sale_date DESC, created_at DESC", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                sales.Add(MapSale(reader));
            }

            return sales;
        }

        public Sale? Read(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = new NpgsqlCommand(@"
                SELECT id, client_id, user_id, sale_date, subtotal, total, status,
                       cancellation_reason, cancelled_at, cancelled_by, created_at
                FROM sales WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapSale(reader);
            }

            return null;
        }

        public List<SaleDetail> GetDetails(Guid saleId)
        {
            var details = new List<SaleDetail>();
            using var conn = _database.GetConnection();
            using var cmd = new NpgsqlCommand(@"
                SELECT id, sale_id, product_id, quantity, unit_price, subtotal
                FROM sale_details WHERE sale_id = @sale_id
                ORDER BY id", conn);
            cmd.Parameters.AddWithValue("@sale_id", NpgsqlDbType.Uuid, saleId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                details.Add(MapSaleDetail(reader));
            }

            return details;
        }

        public void Create(Sale sale)
        {
            using var conn = _database.GetConnection();
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO sales (
                    id, client_id, user_id, sale_date, subtotal, total, status, 
                    cancellation_reason, cancelled_at, cancelled_by
                ) VALUES (
                    @id, @client_id, @user_id, @sale_date, @subtotal, @total, @status,
                    @cancellation_reason, @cancelled_at, @cancelled_by
                )", conn);

            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, sale.Id);
            cmd.Parameters.AddWithValue("@client_id", NpgsqlDbType.Uuid, sale.ClientId);
            cmd.Parameters.AddWithValue("@user_id", NpgsqlDbType.Uuid, sale.UserId);
            cmd.Parameters.AddWithValue("@sale_date", NpgsqlDbType.TimestampTz, sale.SaleDate);
            cmd.Parameters.AddWithValue("@subtotal", NpgsqlDbType.Numeric, sale.Subtotal);
            cmd.Parameters.AddWithValue("@total", NpgsqlDbType.Numeric, sale.Total);
            cmd.Parameters.AddWithValue("@status", NpgsqlDbType.Varchar, sale.Status);
            cmd.Parameters.AddWithValue("@cancellation_reason", NpgsqlDbType.Varchar, (object?)sale.CancellationReason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cancelled_at", NpgsqlDbType.TimestampTz, (object?)sale.CancelledAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cancelled_by", NpgsqlDbType.Uuid, (object?)sale.CancelledBy ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details)
        {
            using var conn = _database.GetConnection();
            foreach (var d in details)
            {
                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO sale_details (id, sale_id, product_id, quantity, unit_price, subtotal)
                    VALUES (@id, @sale_id, @product_id, @quantity, @unit_price, @subtotal)
                ", conn);

                var id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id;
                cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);
                cmd.Parameters.AddWithValue("@sale_id", NpgsqlDbType.Uuid, saleId);
                cmd.Parameters.AddWithValue("@product_id", NpgsqlDbType.Uuid, d.ProductId);
                cmd.Parameters.AddWithValue("@quantity", NpgsqlDbType.Integer, d.Quantity);
                cmd.Parameters.AddWithValue("@unit_price", NpgsqlDbType.Numeric, d.UnitPrice);
                cmd.Parameters.AddWithValue("@subtotal", NpgsqlDbType.Numeric, d.Subtotal);

                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Sale sale)
        {
            using var conn = _database.GetConnection();
            using var cmd = new NpgsqlCommand(@"
                UPDATE sales SET
                    client_id = @client_id,
                    user_id = @user_id,
                    sale_date = @sale_date,
                    subtotal = @subtotal,
                    total = @total,
                    status = @status,
                    cancellation_reason = @cancellation_reason,
                    cancelled_at = @cancelled_at,
                    cancelled_by = @cancelled_by
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, sale.Id);
            cmd.Parameters.AddWithValue("@client_id", NpgsqlDbType.Uuid, sale.ClientId);
            cmd.Parameters.AddWithValue("@user_id", NpgsqlDbType.Uuid, sale.UserId);
            cmd.Parameters.AddWithValue("@sale_date", NpgsqlDbType.TimestampTz, sale.SaleDate);
            cmd.Parameters.AddWithValue("@subtotal", NpgsqlDbType.Numeric, sale.Subtotal);
            cmd.Parameters.AddWithValue("@total", NpgsqlDbType.Numeric, sale.Total);
            cmd.Parameters.AddWithValue("@status", NpgsqlDbType.Varchar, sale.Status);
            cmd.Parameters.AddWithValue("@cancellation_reason", NpgsqlDbType.Varchar, (object?)sale.CancellationReason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cancelled_at", NpgsqlDbType.TimestampTz, (object?)sale.CancelledAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cancelled_by", NpgsqlDbType.Uuid, (object?)sale.CancelledBy ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Delete(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = new NpgsqlCommand("DELETE FROM sales WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);
            cmd.ExecuteNonQuery();
        }

        private static Sale MapSale(NpgsqlDataReader reader)
        {
            var sale = new Sale
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                ClientId = reader.GetGuid(reader.GetOrdinal("client_id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                SaleDate = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("sale_date")),
                Subtotal = reader.GetDecimal(reader.GetOrdinal("subtotal")),
                Total = reader.GetDecimal(reader.GetOrdinal("total")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("created_at"))
            };

            var cancellationReasonIdx = reader.GetOrdinal("cancellation_reason");
            if (!reader.IsDBNull(cancellationReasonIdx))
                sale.CancellationReason = reader.GetString(cancellationReasonIdx);

            var cancelledAtIdx = reader.GetOrdinal("cancelled_at");
            if (!reader.IsDBNull(cancelledAtIdx))
                sale.CancelledAt = reader.GetFieldValue<DateTimeOffset>(cancelledAtIdx);

            var cancelledByIdx = reader.GetOrdinal("cancelled_by");
            if (!reader.IsDBNull(cancelledByIdx))
                sale.CancelledBy = reader.GetGuid(cancelledByIdx);

            return sale;
        }

        private static SaleDetail MapSaleDetail(NpgsqlDataReader reader)
        {
            return new SaleDetail
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                SaleId = reader.GetGuid(reader.GetOrdinal("sale_id")),
                ProductId = reader.GetGuid(reader.GetOrdinal("product_id")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("unit_price")),
                Subtotal = reader.GetDecimal(reader.GetOrdinal("subtotal"))
            };
        }
    }
}

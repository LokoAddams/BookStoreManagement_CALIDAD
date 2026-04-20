using System;
using System.Collections.Generic;
using System.Data;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using Npgsql;
using NpgsqlTypes;

namespace MicroServiceSales.Infrastructure.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private const string InsertSaleSql = @"
                INSERT INTO sales (
                    id, client_id, user_id, sale_date, subtotal, total, status, 
                    cancellation_reason, cancelled_at, cancelled_by
                ) VALUES (
                    @id, @client_id, @user_id, @sale_date, @subtotal, @total, @status,
                    @cancellation_reason, @cancelled_at, @cancelled_by
                )";

        private const string InsertSaleDetailSql = @"
                    INSERT INTO sale_details (id, sale_id, product_id, quantity, unit_price, subtotal)
                    VALUES (@id, @sale_id, @product_id, @quantity, @unit_price, @subtotal)
                ";

        private const string UpdateSaleSql = @"
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
                WHERE id = @id";

        private const string DeleteSaleSql = "DELETE FROM sales WHERE id = @id";

        private readonly IDataBase _database;
        private readonly Func<NpgsqlConnection, NpgsqlCommand> _createSaleInsertCommand;
        private readonly Func<NpgsqlConnection, NpgsqlCommand> _createDetailInsertCommand;
        private readonly Func<NpgsqlConnection, NpgsqlCommand> _createSaleUpdateCommand;
        private readonly Func<NpgsqlConnection, NpgsqlCommand> _createSaleDeleteCommand;
        private readonly Action<NpgsqlCommand> _executeNonQuery;
        private readonly Func<NpgsqlConnection, List<Sale>> _getAllSales;
        private readonly Func<NpgsqlConnection, Guid, Sale?> _readSaleById;
        private readonly Func<NpgsqlConnection, Guid, List<SaleDetail>> _getDetailsBySaleId;

        public SalesRepository(
            IDataBase database,
            Func<NpgsqlConnection, NpgsqlCommand>? createSaleInsertCommand = null,
            Func<NpgsqlConnection, NpgsqlCommand>? createDetailInsertCommand = null,
            Func<NpgsqlConnection, NpgsqlCommand>? createSaleUpdateCommand = null,
            Func<NpgsqlConnection, NpgsqlCommand>? createSaleDeleteCommand = null,
            Action<NpgsqlCommand>? executeNonQuery = null,
            Func<NpgsqlConnection, List<Sale>>? getAllSales = null,
            Func<NpgsqlConnection, Guid, Sale?>? readSaleById = null,
            Func<NpgsqlConnection, Guid, List<SaleDetail>>? getDetailsBySaleId = null)
        {
            _database = database;
            _createSaleInsertCommand = createSaleInsertCommand ?? (conn => new NpgsqlCommand(InsertSaleSql, conn));
            _createDetailInsertCommand = createDetailInsertCommand ?? (conn => new NpgsqlCommand(InsertSaleDetailSql, conn));
            _createSaleUpdateCommand = createSaleUpdateCommand ?? (conn => new NpgsqlCommand(UpdateSaleSql, conn));
            _createSaleDeleteCommand = createSaleDeleteCommand ?? (conn => new NpgsqlCommand(DeleteSaleSql, conn));
            _executeNonQuery = executeNonQuery ?? (cmd => cmd.ExecuteNonQuery());
            _getAllSales = getAllSales ?? GetAllFromDatabase;
            _readSaleById = readSaleById ?? ReadSaleByIdFromDatabase;
            _getDetailsBySaleId = getDetailsBySaleId ?? GetDetailsBySaleIdFromDatabase;
        }

        public List<Sale> GetAll()
        {
            using var conn = _database.GetConnection();
            return _getAllSales(conn);
        }

        private static List<Sale> GetAllFromDatabase(NpgsqlConnection conn)
        {
            var sales = new List<Sale>();
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
            return _readSaleById(conn, id);
        }

        private static Sale? ReadSaleByIdFromDatabase(NpgsqlConnection conn, Guid id)
        {
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
            using var conn = _database.GetConnection();
            return _getDetailsBySaleId(conn, saleId);
        }

        private static List<SaleDetail> GetDetailsBySaleIdFromDatabase(NpgsqlConnection conn, Guid saleId)
        {
            var details = new List<SaleDetail>();
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
            using var cmd = _createSaleInsertCommand(conn);

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

            _executeNonQuery(cmd);
        }

        public void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details)
        {
            using var conn = _database.GetConnection();
            foreach (var d in details)
            {
                using var cmd = _createDetailInsertCommand(conn);

                var id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id;
                cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);
                cmd.Parameters.AddWithValue("@sale_id", NpgsqlDbType.Uuid, saleId);
                cmd.Parameters.AddWithValue("@product_id", NpgsqlDbType.Uuid, d.ProductId);
                cmd.Parameters.AddWithValue("@quantity", NpgsqlDbType.Integer, d.Quantity);
                cmd.Parameters.AddWithValue("@unit_price", NpgsqlDbType.Numeric, d.UnitPrice);
                cmd.Parameters.AddWithValue("@subtotal", NpgsqlDbType.Numeric, d.Subtotal);

                _executeNonQuery(cmd);
            }
        }

        public void Update(Sale sale)
        {
            using var conn = _database.GetConnection();
            using var cmd = _createSaleUpdateCommand(conn);

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

            _executeNonQuery(cmd);
        }

        public void Delete(Guid id)
        {
            using var conn = _database.GetConnection();
            using var cmd = _createSaleDeleteCommand(conn);
            cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);

            _executeNonQuery(cmd);
        }

        private static Sale MapSale(NpgsqlDataReader reader)
        {
            return MapSaleRecord(reader);
        }

        internal static Sale MapSaleRecord(System.Data.IDataRecord record)
        {
            var sale = new Sale
            {
                Id = record.GetGuid(record.GetOrdinal("id")),
                ClientId = record.GetGuid(record.GetOrdinal("client_id")),
                UserId = record.GetGuid(record.GetOrdinal("user_id")),
                SaleDate = (DateTimeOffset)record.GetValue(record.GetOrdinal("sale_date")),
                Subtotal = record.GetDecimal(record.GetOrdinal("subtotal")),
                Total = record.GetDecimal(record.GetOrdinal("total")),
                Status = record.GetString(record.GetOrdinal("status")),
                CreatedAt = (DateTimeOffset)record.GetValue(record.GetOrdinal("created_at"))
            };

            var cancellationReasonIdx = record.GetOrdinal("cancellation_reason");
            if (!record.IsDBNull(cancellationReasonIdx))
                sale.CancellationReason = record.GetString(cancellationReasonIdx);

            var cancelledAtIdx = record.GetOrdinal("cancelled_at");
            if (!record.IsDBNull(cancelledAtIdx))
                sale.CancelledAt = (DateTimeOffset)record.GetValue(cancelledAtIdx);

            var cancelledByIdx = record.GetOrdinal("cancelled_by");
            if (!record.IsDBNull(cancelledByIdx))
                sale.CancelledBy = record.GetGuid(cancelledByIdx);

            return sale;
        }

        private static SaleDetail MapSaleDetail(NpgsqlDataReader reader)
        {
            return MapSaleDetailRecord(reader);
        }

        internal static SaleDetail MapSaleDetailRecord(IDataRecord record)
        {
            return new SaleDetail
            {
                Id = record.GetGuid(record.GetOrdinal("id")),
                SaleId = record.GetGuid(record.GetOrdinal("sale_id")),
                ProductId = record.GetGuid(record.GetOrdinal("product_id")),
                Quantity = record.GetInt32(record.GetOrdinal("quantity")),
                UnitPrice = record.GetDecimal(record.GetOrdinal("unit_price")),
                Subtotal = record.GetDecimal(record.GetOrdinal("subtotal"))
            };
        }
    }
}

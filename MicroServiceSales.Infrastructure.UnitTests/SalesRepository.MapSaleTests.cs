using System.Data;
using MicroServiceSales.Infrastructure.Repositories;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryMapSaleTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void MapSale_Should_Map_Cancellation_Fields_By_Nullability(bool reasonIsNull, bool atIsNull, bool byIsNull)
    {
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var saleDate = DateTimeOffset.UtcNow;
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        const decimal subtotal = 80m;
        const decimal total = 80m;
        const string status = "PENDING";

        const string cancellationReason = "Cliente solicito cancelacion";
        var cancelledAt = DateTimeOffset.UtcNow;
        var cancelledBy = Guid.NewGuid();

        var table = new DataTable();
        table.Columns.Add("id", typeof(Guid));
        table.Columns.Add("client_id", typeof(Guid));
        table.Columns.Add("user_id", typeof(Guid));
        table.Columns.Add("sale_date", typeof(DateTimeOffset));
        table.Columns.Add("subtotal", typeof(decimal));
        table.Columns.Add("total", typeof(decimal));
        table.Columns.Add("status", typeof(string));
        table.Columns.Add("created_at", typeof(DateTimeOffset));
        table.Columns.Add("cancellation_reason", typeof(string));
        table.Columns.Add("cancelled_at", typeof(DateTimeOffset));
        table.Columns.Add("cancelled_by", typeof(Guid));

        table.Rows.Add(
            id,
            clientId,
            userId,
            saleDate,
            subtotal,
            total,
            status,
            createdAt,
            reasonIsNull ? DBNull.Value : cancellationReason,
            atIsNull ? DBNull.Value : cancelledAt,
            byIsNull ? DBNull.Value : cancelledBy);

        using var reader = table.CreateDataReader();
        Assert.True(reader.Read());

        var sale = SalesRepository.MapSaleRecord(reader);

        Assert.Equal(id, sale.Id);
        Assert.Equal(clientId, sale.ClientId);
        Assert.Equal(userId, sale.UserId);
        Assert.Equal(saleDate, sale.SaleDate);
        Assert.Equal(subtotal, sale.Subtotal);
        Assert.Equal(total, sale.Total);
        Assert.Equal(status, sale.Status);
        Assert.Equal(createdAt, sale.CreatedAt);

        if (reasonIsNull)
            Assert.Null(sale.CancellationReason);
        else
            Assert.Equal(cancellationReason, sale.CancellationReason);

        if (atIsNull)
            Assert.Null(sale.CancelledAt);
        else
            Assert.Equal(cancelledAt, sale.CancelledAt);

        if (byIsNull)
            Assert.Null(sale.CancelledBy);
        else
            Assert.Equal(cancelledBy, sale.CancelledBy);
    }
}

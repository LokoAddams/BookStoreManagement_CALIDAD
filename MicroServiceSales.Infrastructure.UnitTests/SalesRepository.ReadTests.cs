using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Infrastructure.Repositories;
using Xunit;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryReadTests
{
    [Fact]
    public void Read_Should_Return_Mapped_Sale_When_Record_Exists()
    {
        var database = new FakeDatabase();
        var existingId = Guid.NewGuid();
        var expected = new Sale
        {
            Id = existingId,
            ClientId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SaleDate = DateTimeOffset.UtcNow,
            Subtotal = 100m,
            Total = 100m,
            Status = "PENDING",
            CreatedAt = DateTimeOffset.UtcNow
        };
        Guid? capturedId = null;

        var repository = new SalesRepository(
            database,
            readSaleById: (_, id) =>
            {
                capturedId = id;
                return expected;
            });

        var result = repository.Read(existingId);

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Equal(existingId, capturedId);
        Assert.NotNull(result);
        Assert.Equal(expected.Id, result!.Id);
        Assert.Equal(expected.ClientId, result.ClientId);
        Assert.Equal(expected.UserId, result.UserId);
        Assert.Equal(expected.Status, result.Status);
    }

    [Fact]
    public void Read_Should_Return_Null_When_Record_Does_Not_Exist()
    {
        var database = new FakeDatabase();
        var missingId = Guid.NewGuid();
        Guid? capturedId = null;

        var repository = new SalesRepository(
            database,
            readSaleById: (_, id) =>
            {
                capturedId = id;
                return null;
            });

        var result = repository.Read(missingId);

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Equal(missingId, capturedId);
        Assert.Null(result);
    }
}

internal sealed class FakeDatabase : IDataBase
{
    public int GetConnectionCalls { get; private set; }

    public Npgsql.NpgsqlConnection GetConnection()
    {
        GetConnectionCalls++;
        return new Npgsql.NpgsqlConnection();
    }
}

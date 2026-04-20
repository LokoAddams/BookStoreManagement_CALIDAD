using MicroServiceSales.Domain.Models;
using MicroServiceSales.Infrastructure.Repositories;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryGetAllTests
{
    [Fact]
    public void GetAll_Should_Return_Mapped_Sales_When_Table_Has_At_Least_One_Record()
    {
        var database = new FakeDatabase();
        var first = CreateSale();
        var second = CreateSale();

        var repository = new SalesRepository(
            database,
            getAllSales: _ => [first, second]);

        var result = repository.GetAll();

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Equal(2, result.Count);
        Assert.Equal(first.Id, result[0].Id);
        Assert.Equal(first.ClientId, result[0].ClientId);
        Assert.Equal(first.Status, result[0].Status);
        Assert.Equal(second.Id, result[1].Id);
        Assert.Equal(second.ClientId, result[1].ClientId);
        Assert.Equal(second.Status, result[1].Status);
    }

    [Fact]
    public void GetAll_Should_Return_Empty_List_When_Table_Is_Empty()
    {
        var database = new FakeDatabase();

        var repository = new SalesRepository(
            database,
            getAllSales: _ => []);

        var result = repository.GetAll();

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private static Sale CreateSale()
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SaleDate = DateTimeOffset.UtcNow,
            Subtotal = 100m,
            Total = 100m,
            Status = "PENDING",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

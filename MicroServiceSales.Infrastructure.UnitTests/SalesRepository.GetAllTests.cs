using MicroServiceSales.Domain.Models;
using MicroServiceSales.Infrastructure.Repositories;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryGetAllTests
{
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

    [Fact]
    public void GetAll_Should_Return_One_Sale_When_Table_Has_One_Record()
    {
        var database = new FakeDatabase();
        var expected = CreateSale();

        var repository = new SalesRepository(
            database,
            getAllSales: _ => [expected]);

        var result = repository.GetAll();

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Single(result);
        Assert.Equal(expected.Id, result[0].Id);
        Assert.Equal(expected.ClientId, result[0].ClientId);
        Assert.Equal(expected.Status, result[0].Status);
    }

    [Fact]
    public void GetAll_Should_Return_All_Sales_When_Table_Has_Multiple_Records()
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
        Assert.Equal(second.Id, result[1].Id);
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

using MicroServiceSales.Domain.Models;
using MicroServiceSales.Infrastructure.Repositories;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryGetDetailsTests
{
    [Fact]
    public void GetDetails_Should_Return_Empty_List_When_No_Details_Exist()
    {
        var database = new FakeDatabase();
        var saleId = Guid.NewGuid();
        Guid? capturedSaleId = null;

        var repository = new SalesRepository(
            database,
            getDetailsBySaleId: (_, id) =>
            {
                capturedSaleId = id;
                return [];
            });

        var result = repository.GetDetails(saleId);

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Equal(saleId, capturedSaleId);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetDetails_Should_Return_One_Detail_When_One_Detail_Exists()
    {
        var database = new FakeDatabase();
        var saleId = Guid.NewGuid();
        Guid? capturedSaleId = null;
        var expectedDetail = CreateDetail(saleId);

        var repository = new SalesRepository(
            database,
            getDetailsBySaleId: (_, id) =>
            {
                capturedSaleId = id;
                return [expectedDetail];
            });

        var result = repository.GetDetails(saleId);

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Equal(saleId, capturedSaleId);
        Assert.Single(result);
        Assert.Equal(expectedDetail.Id, result[0].Id);
        Assert.Equal(expectedDetail.ProductId, result[0].ProductId);
    }

    [Fact]
    public void GetDetails_Should_Return_All_Details_When_Multiple_Details_Exist()
    {
        var database = new FakeDatabase();
        var saleId = Guid.NewGuid();
        Guid? capturedSaleId = null;
        var first = CreateDetail(saleId);
        var second = CreateDetail(saleId);

        var repository = new SalesRepository(
            database,
            getDetailsBySaleId: (_, id) =>
            {
                capturedSaleId = id;
                return [first, second];
            });

        var result = repository.GetDetails(saleId);

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Equal(saleId, capturedSaleId);
        Assert.Equal(2, result.Count);
        Assert.Equal(first.Id, result[0].Id);
        Assert.Equal(second.Id, result[1].Id);
    }

    private static SaleDetail CreateDetail(Guid saleId)
    {
        return new SaleDetail
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            UnitPrice = 10m,
            Subtotal = 20m
        };
    }
}

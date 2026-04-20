using MicroServiceSales.Application.Services;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;

namespace MicroServiceSales.Application.UnitTests;

public class SalesServiceUpdateTests
{
    [Fact]
    public void Update_Should_Normalize_Text_And_Call_Repository_When_Sale_Is_Valid()
    {
        var repository = new FakeSalesRepository();
        var service = new SalesService(repository, new NullEventPublisher());
        var sale = CreateValidSale();

        service.Update(sale);

        Assert.Equal(1, repository.UpdateCalls);
        Assert.Same(sale, repository.LastUpdatedSale);
        Assert.Equal("PENDING", sale.Status);
        Assert.Equal(100.13m, sale.Subtotal);
        Assert.Equal(100.13m, sale.Total);
    }

    [Fact]
    public void Update_Should_Throw_ValidationException_And_Not_Call_Repository_When_Sale_Is_Invalid()
    {
        var repository = new FakeSalesRepository();
        var service = new SalesService(repository, new NullEventPublisher());
        var sale = CreateInvalidSale();

        var exception = Assert.Throws<ValidationException>(() => service.Update(sale));

        Assert.NotEmpty(exception.Errors);
        Assert.Equal(0, repository.UpdateCalls);
        Assert.Null(repository.LastUpdatedSale);
    }

    private static Sale CreateValidSale()
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SaleDate = DateTimeOffset.UtcNow,
            Subtotal = 100.126m,
            Total = 100.126m,
            Status = "   PENDING   ",
            CreatedAt = DateTimeOffset.UtcNow,
            Details = []
        };
    }

    private static Sale CreateInvalidSale()
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Empty,
            UserId = Guid.Empty,
            SaleDate = default,
            Subtotal = -1m,
            Total = -2m,
            Status = "INVALID",
            CreatedAt = default,
            Details = []
        };
    }

    private sealed class FakeSalesRepository : ISalesRepository
    {
        public int UpdateCalls { get; private set; }
        public Sale? LastUpdatedSale { get; private set; }

        public List<Sale> GetAll() => [];
        public Sale? Read(Guid id) => null;
        public List<SaleDetail> GetDetails(Guid saleId) => [];
        public void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details) { }
        public void Create(Sale sale) { }

        public void Update(Sale sale)
        {
            UpdateCalls++;
            LastUpdatedSale = sale;
        }

        public void Delete(Guid id) { }
    }
}

using MicroServiceSales.Application.Services;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Application.UnitTests;

public class SalesServiceGetDetailsTests
{
    [Fact]
    public void GetDetails_Should_Return_Detail_List_When_Sale_Has_Associated_Records()
    {
        var saleIdWithDetails = Guid.NewGuid();
        var expected = new List<SaleDetail>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SaleId = saleIdWithDetails,
                ProductId = Guid.NewGuid(),
                ProductName = "Producto A",
                Quantity = 2,
                UnitPrice = 10m,
                Subtotal = 20m
            },
            new()
            {
                Id = Guid.NewGuid(),
                SaleId = saleIdWithDetails,
                ProductId = Guid.NewGuid(),
                ProductName = "Producto B",
                Quantity = 1,
                UnitPrice = 5m,
                Subtotal = 5m
            }
        };

        var repository = new FakeSalesRepository(new Dictionary<Guid, List<SaleDetail>>
        {
            [saleIdWithDetails] = expected
        });
        var service = new SalesService(repository, new NullEventPublisher());

        var result = service.GetDetails(saleIdWithDetails);

        Assert.Equal(1, repository.GetDetailsCalls);
        Assert.Equal(saleIdWithDetails, repository.LastRequestedSaleId);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, detail => Assert.Equal(saleIdWithDetails, detail.SaleId));
        Assert.Equal(expected[0].Id, result[0].Id);
        Assert.Equal(expected[1].Id, result[1].Id);
    }

    [Fact]
    public void GetDetails_Should_Return_Empty_List_When_Sale_Has_No_Associated_Records()
    {
        var saleIdWithNoDetails = Guid.NewGuid();
        var repository = new FakeSalesRepository(new Dictionary<Guid, List<SaleDetail>>());
        var service = new SalesService(repository, new NullEventPublisher());

        var result = service.GetDetails(saleIdWithNoDetails);

        Assert.Equal(1, repository.GetDetailsCalls);
        Assert.Equal(saleIdWithNoDetails, repository.LastRequestedSaleId);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private sealed class FakeSalesRepository(Dictionary<Guid, List<SaleDetail>> detailsBySaleId) : ISalesRepository
    {
        public int GetDetailsCalls { get; private set; }
        public Guid LastRequestedSaleId { get; private set; }

        public List<Sale> GetAll() => [];
        public Sale? Read(Guid id) => null;

        public List<SaleDetail> GetDetails(Guid saleId)
        {
            GetDetailsCalls++;
            LastRequestedSaleId = saleId;
            return detailsBySaleId.TryGetValue(saleId, out var details)
                ? details
                : [];
        }

        public void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details) { }
        public void Create(Sale sale) { }
        public void Update(Sale sale) { }
        public void Delete(Guid id) { }
    }

    private sealed class NullEventPublisher : IEventPublisher
    {
        public Task PublishAsync(string routingKey, object @event) => Task.CompletedTask;
    }
}

using MicroServiceSales.Application.Services;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Application.UnitTests;

public class SalesServiceReadTests
{
    [Fact]
    public void Read_Should_Return_Sale_Instance_When_Id_Exists_In_Repository()
    {
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

        var repository = new FakeSalesRepository(new Dictionary<Guid, Sale>
        {
            [existingId] = expected
        });
        var service = new SalesService(repository, new NullEventPublisher());

        var result = service.Read(existingId);

        Assert.Equal(1, repository.ReadCalls);
        Assert.Equal(existingId, repository.LastReadId);
        Assert.NotNull(result);
        Assert.Equal(expected.Id, result!.Id);
        Assert.Equal(expected.ClientId, result.ClientId);
        Assert.Equal(expected.Status, result.Status);
    }

    [Fact]
    public void Read_Should_Return_Null_When_Id_Does_Not_Exist_In_Repository()
    {
        var existingId = Guid.NewGuid();
        var missingId = Guid.NewGuid();

        var repository = new FakeSalesRepository(new Dictionary<Guid, Sale>
        {
            [existingId] = new Sale
            {
                Id = existingId,
                ClientId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SaleDate = DateTimeOffset.UtcNow,
                Subtotal = 50m,
                Total = 50m,
                Status = "PENDING",
                CreatedAt = DateTimeOffset.UtcNow
            }
        });
        var service = new SalesService(repository, new NullEventPublisher());

        var result = service.Read(missingId);

        Assert.Equal(1, repository.ReadCalls);
        Assert.Equal(missingId, repository.LastReadId);
        Assert.Null(result);
    }

    private sealed class FakeSalesRepository(Dictionary<Guid, Sale> data) : ISalesRepository
    {
        public int ReadCalls { get; private set; }
        public Guid LastReadId { get; private set; }

        public List<Sale> GetAll() => [];

        public Sale? Read(Guid id)
        {
            ReadCalls++;
            LastReadId = id;
            return data.TryGetValue(id, out var sale) ? sale : null;
        }

        public List<SaleDetail> GetDetails(Guid saleId) => [];
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

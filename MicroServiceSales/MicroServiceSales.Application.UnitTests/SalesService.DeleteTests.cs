using MicroServiceSales.Application.Services;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Application.UnitTests;

public class SalesServiceDeleteTests
{
    [Fact]
    public void Delete_Should_Call_Repository_Delete_When_Id_Exists()
    {
        var existingId = Guid.NewGuid();
        var repository = new FakeSalesRepository([existingId]);
        var service = new SalesService(repository, new NullEventPublisher());

        service.Delete(existingId);

        Assert.Equal(1, repository.DeleteCalls);
        Assert.Equal(existingId, repository.LastDeletedId);
    }

    [Fact]
    public void Delete_Should_Call_Repository_Delete_When_Id_Does_Not_Exist()
    {
        var existingId = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        var repository = new FakeSalesRepository([existingId]);
        var service = new SalesService(repository, new NullEventPublisher());

        service.Delete(missingId);

        Assert.Equal(1, repository.DeleteCalls);
        Assert.Equal(missingId, repository.LastDeletedId);
    }

    private sealed class FakeSalesRepository(IEnumerable<Guid> existingIds) : ISalesRepository
    {
        private readonly HashSet<Guid> _existingIds = [.. existingIds];

        public int DeleteCalls { get; private set; }
        public Guid LastDeletedId { get; private set; }

        public List<Sale> GetAll() => [];
        public Sale? Read(Guid id) => null;
        public List<SaleDetail> GetDetails(Guid saleId) => [];
        public void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details) { }
        public void Create(Sale sale) { }
        public void Update(Sale sale) { }

        public void Delete(Guid id)
        {
            DeleteCalls++;
            LastDeletedId = id;
            _existingIds.Remove(id);
        }
    }

    private sealed class NullEventPublisher : IEventPublisher
    {
        public Task PublishAsync(string routingKey, object @event) => Task.CompletedTask;
    }
}

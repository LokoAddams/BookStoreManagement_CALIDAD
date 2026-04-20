using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Application.UnitTests;

internal sealed class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync(string routingKey, object @event) => Task.CompletedTask;
}

internal abstract class FakeSalesRepositoryBase : ISalesRepository
{
    public virtual List<Sale> GetAll() => [];

    public virtual Sale? Read(Guid id) => null;

    public virtual List<SaleDetail> GetDetails(Guid saleId) => [];

    public virtual void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details) { }

    public virtual void Create(Sale sale) { }

    public virtual void Update(Sale sale) { }

    public virtual void Delete(Guid id) { }
}
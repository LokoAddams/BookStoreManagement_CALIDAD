using MicroServiceSales.Application.Services;
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

    private sealed class FakeSalesRepository(IEnumerable<Guid> existingIds) : FakeSalesRepositoryBase
    {
        private readonly HashSet<Guid> _existingIds = [.. existingIds];

        public int DeleteCalls { get; private set; }
        public Guid LastDeletedId { get; private set; }

        public override void Delete(Guid id)
        {
            DeleteCalls++;
            LastDeletedId = id;
            _existingIds.Remove(id);
        }
    }
}

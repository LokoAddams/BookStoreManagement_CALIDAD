using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Domain.Models;
using NUnit.Framework;
using ServiceCommon.Domain.Models;

namespace MicroServiceProduct.NUnitTests;

[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
public class ProductServiceNUnitTests
{
    private readonly FakeProductRepository _repo = new();
    private ProductService _service = null!;
    private int _instanceCounter;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _service = new ProductService(_repo);
    }

    [Test]
    public void CreateAndRead_ShouldReturnStoredProduct()
    {
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Libro DDD", Description = "Demo", CategoryId = Guid.NewGuid(), Price = 42, Stock = 7 };

        _service.Create(product);
        var loaded = _service.Read(id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Name, Is.EqualTo("Libro DDD"));
        Assert.That(_repo.CreateCallCount, Is.EqualTo(1));
    }

    [Test, Order(1)]
    public void InstanceState_WithSingleFixture_ShouldAccumulate_1()
    {
        Assert.That(_instanceCounter, Is.EqualTo(0));
        _instanceCounter++;
        Assert.That(_instanceCounter, Is.EqualTo(1));
    }

    [Test, Order(2)]
    public void InstanceState_WithSingleFixture_ShouldAccumulate_2()
    {
        Assert.That(_instanceCounter, Is.EqualTo(1));
        _instanceCounter++;
        Assert.That(_instanceCounter, Is.EqualTo(2));
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _store = new();
        public int CreateCallCount { get; private set; }

        public void Create(Product product)
        {
            CreateCallCount++;
            _store[product.Id] = product;
        }

        public Product? Read(Guid id) => _store.TryGetValue(id, out var product) ? product : null;
        public void Update(Product product) => _store[product.Id] = product;
        public void Delete(Guid id) => _store.Remove(id);
        public List<Product> GetAll() => _store.Values.ToList();
        public bool TryReserveStock(Dictionary<Guid, int> items, out string? error)
        {
            error = null;
            return true;
        }

        public Task<int> CountAsync(CancellationToken ct = default) => Task.FromResult(_store.Count);

        public Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
            => Task.FromResult(new PagedResult<Product>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = _store.Count,
                Items = _store.Values.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            });
    }
}

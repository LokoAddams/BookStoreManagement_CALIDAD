using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Domain.Models;
using ServiceCommon.Domain.Models;

namespace XUnit;

public class ProductServiceXUnitTests
{
    private readonly FakeProductRepository _repo = new();
    private readonly ProductService _service;
    private int _instanceCounter;

    public ProductServiceXUnitTests()
    {
        _service = new ProductService(_repo);
    }

    [Fact]
    public void CreateAndRead_ShouldReturnStoredProduct()
    {
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Libro Clean Code", Description = "Demo", CategoryId = Guid.NewGuid(), Price = 30, Stock = 4 };

        _service.Create(product);
        var loaded = _service.Read(id);

        Assert.NotNull(loaded);
        Assert.Equal("Libro Clean Code", loaded!.Name);
        Assert.Equal(1, _repo.CreateCallCount);
    }

    [Fact]
    public void InstanceState_ShouldStartClean_InEachTest_1()
    {
        Assert.Equal(0, _instanceCounter);
        _instanceCounter++;
        Assert.Equal(1, _instanceCounter);
    }

    [Fact]
    public void InstanceState_ShouldStartClean_InEachTest_2()
    {
        Assert.Equal(0, _instanceCounter);
        _instanceCounter++;
        Assert.Equal(1, _instanceCounter);
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

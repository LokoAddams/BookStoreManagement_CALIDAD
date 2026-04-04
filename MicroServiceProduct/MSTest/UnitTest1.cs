using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Domain.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceCommon.Domain.Models;

namespace MicroServiceProduct.MSTestTests;

[TestClass]
public class ProductServiceMSTestTests
{
    private static readonly FakeProductRepository Repo = new();
    private static ProductService _service = null!;
    private static int _sharedCounter;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        _service = new ProductService(Repo);
        _sharedCounter = 0;
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _sharedCounter = 0;
    }

    [TestMethod]
    public void CreateAndRead_ShouldReturnStoredProduct()
    {
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Libro Patterns", Description = "Demo", CategoryId = Guid.NewGuid(), Price = 15, Stock = 10 };

        _service.Create(product);
        var loaded = _service.Read(id);

        Assert.IsNotNull(loaded);
        Assert.AreEqual("Libro Patterns", loaded!.Name);
        Assert.AreEqual(1, Repo.CreateCallCount);
    }

    [TestMethod]
    public void SharedState_WithClassInitialize_ShouldAccumulate_1()
    {
        Assert.AreEqual(0, _sharedCounter);
        _sharedCounter++;
        Assert.AreEqual(1, _sharedCounter);
    }

    [TestMethod]
    public void SharedState_WithClassInitialize_ShouldAccumulate_2()
    {
        if (_sharedCounter == 0)
        {
            _sharedCounter++;
        }

        Assert.AreEqual(1, _sharedCounter);
        _sharedCounter++;
        Assert.AreEqual(2, _sharedCounter);
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

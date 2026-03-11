// Removed: reporting service and all report-related code per user request.
// File intentionally left blank.
// New file: ProductService implementation
using System;
using System.Collections.Generic;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Domain.Models;
using ServiceCommon.Domain.Models;

namespace MicroServiceProduct.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public void Create(Product product) => _repo.Create(product);

        public Product? Read(Guid id) => _repo.Read(id);

        public void Update(Product product) => _repo.Update(product);

        public void Delete(Guid id) => _repo.Delete(id);

        public List<Product> GetAll() => _repo.GetAll();

        public bool TryReserveStock(Dictionary<Guid, int> items, out string? error)
            => _repo.TryReserveStock(items, out error);
        Task<PagedResult<Product>> IProductService.GetPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            return _repo.GetPagedAsync(page, pageSize, ct);
        }

    }
}

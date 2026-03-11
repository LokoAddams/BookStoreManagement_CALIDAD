// New file: minimal IProductRepository
using MicroServiceProduct.Domain.Models;
using ServiceCommon.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceProduct.Domain.Interfaces
{
    public interface IProductRepository
    {
        void Create(Product product);
        Product? Read(Guid id);
        void Update(Product product);
        void Delete(Guid id);
        List<Product> GetAll();
        bool TryReserveStock(Dictionary<Guid, int> items, out string? error);
        Task<int> CountAsync(CancellationToken ct = default);
        Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    }
}

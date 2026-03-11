using System;
using System.Collections.Generic;
using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Domain.Interfaces
{
    public interface ISalesService
    {
        List<Sale> GetAll();
        Sale? Read(Guid id);
        List<SaleDetail> GetDetails(Guid saleId);
        void Create(Sale sale);
        void Update(Sale sale);
        void Delete(Guid id);
    }
}

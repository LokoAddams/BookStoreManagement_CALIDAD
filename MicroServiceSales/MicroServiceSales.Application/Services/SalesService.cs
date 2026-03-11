using System;
using System.Collections.Generic;
using System.Linq;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;

namespace MicroServiceSales.Application.Services
{
    public class SalesService : ISalesService
    {
        private readonly ISalesRepository _repository;
        private readonly IEventPublisher _publisher;

        public SalesService(ISalesRepository repository, IEventPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public List<Sale> GetAll() => _repository.GetAll();

        public Sale? Read(Guid id) => _repository.Read(id);

        public List<SaleDetail> GetDetails(Guid saleId) => _repository.GetDetails(saleId);

        public void Create(Sale sale)
        {
            // Asegurar que la venta tenga un ID
            if (sale.Id == Guid.Empty)
                sale.Id = Guid.NewGuid();
            
            // Orquestación: nueva venta se crea en estado PENDING
            sale.Status = "PENDING";
            
            // Preparar los detalles ANTES de validar
            if (sale.Details != null && sale.Details.Any())
            {
                foreach (var d in sale.Details)
                {
                    // Asegurar IDs y subtotal coherente
                    if (d.Id == Guid.Empty)
                        d.Id = Guid.NewGuid();
                    d.SaleId = sale.Id;
                    if (d.Subtotal == 0)
                        d.Subtotal = d.UnitPrice * d.Quantity;
                }
            }
            
            // AHORA validamos con todos los datos completos
            var errors = SalesValidation.ValidateAll(sale).ToList();
            if (errors.Any())
                throw new ValidationException(errors);

            SalesValidation.Normalize(sale);
            // NO guardamos en DB todavía, esperamos aprobación de productos

            // Publicar evento a sales.pending para que productos verifique stock
            var evt = new
            {
                MessageId = Guid.NewGuid().ToString(),
                SaleId = sale.Id,
                UserId = sale.UserId,
                UserName = sale.UserName,
                ClientId = sale.ClientId,
                ClientName = sale.ClientName,
                ClientCi = sale.ClientCi,
                Subtotal = sale.Subtotal,
                Total = sale.Total,
                SaleDate = sale.SaleDate,
                Products = (sale.Details ?? new List<SaleDetail>()).Select(d => new
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };
            // Publicar a sales.pending para iniciar la saga
            _publisher.PublishAsync("sales.pending", evt);
        }

        public void Update(Sale sale)
        {
            var errors = SalesValidation.ValidateAll(sale).ToList();
            if (errors.Any())
                throw new ValidationException(errors);

            SalesValidation.Normalize(sale);
            _repository.Update(sale);
        }

        public void Delete(Guid id) => _repository.Delete(id);
    }
}

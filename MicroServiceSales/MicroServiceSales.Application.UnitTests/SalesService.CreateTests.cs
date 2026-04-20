using MicroServiceSales.Application.Services;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;

namespace MicroServiceSales.Application.UnitTests;

public class SalesServiceCreateTests
{
    [Fact]
    public void Create_Should_Generate_Sale_And_Detail_Ids_And_Subtotal_When_Both_Are_Empty_Or_Zero()
    {
        var repository = new FakeSalesRepository();
        var publisher = new FakeEventPublisher();
        var service = new SalesService(repository, publisher);
        var sale = CreateValidSale(Guid.Empty, new List<SaleDetail>
        {
            CreateDetail(Guid.Empty, Guid.Empty, 2, 25m, 0m)
        });

        service.Create(sale);

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal("PENDING", sale.Status);
        Assert.Single(sale.Details);
        Assert.NotEqual(Guid.Empty, sale.Details[0].Id);
        Assert.Equal(sale.Id, sale.Details[0].SaleId);
        Assert.Equal(50m, sale.Details[0].Subtotal);
        Assert.Equal("sales.pending", publisher.RoutingKey);
        Assert.Equal(1, publisher.CallCount);
        Assert.Empty(repository.CreatedSales);
        Assert.Empty(repository.CreatedDetailsCalls);
    }

    [Fact]
    public void Create_Should_Keep_Existing_Sale_And_Detail_Values_When_Already_Present()
    {
        var repository = new FakeSalesRepository();
        var publisher = new FakeEventPublisher();
        var service = new SalesService(repository, publisher);
        var saleId = Guid.NewGuid();
        var detailId = Guid.NewGuid();
        var sale = CreateValidSale(saleId, new List<SaleDetail>
        {
            CreateDetail(detailId, Guid.Empty, 2, 25m, 50m)
        });

        service.Create(sale);

        Assert.Equal(saleId, sale.Id);
        Assert.Equal("PENDING", sale.Status);
        Assert.Equal(detailId, sale.Details[0].Id);
        Assert.Equal(saleId, sale.Details[0].SaleId);
        Assert.Equal(50m, sale.Details[0].Subtotal);
        Assert.Equal("sales.pending", publisher.RoutingKey);
        Assert.Equal(1, publisher.CallCount);
    }

    [Fact]
    public void Create_Should_Allow_Sale_Without_Details_And_Publish_Empty_Products_List()
    {
        var repository = new FakeSalesRepository();
        var publisher = new FakeEventPublisher();
        var service = new SalesService(repository, publisher);
        var sale = CreateValidSale(Guid.Empty, new List<SaleDetail>());

        service.Create(sale);

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal("PENDING", sale.Status);
        Assert.Empty(sale.Details);
        Assert.Equal("sales.pending", publisher.RoutingKey);
        Assert.Equal(1, publisher.CallCount);
        Assert.Empty(repository.CreatedSales);
        Assert.Empty(repository.CreatedDetailsCalls);
        Assert.Empty(publisher.LastProducts);
    }

    [Fact]
    public void Create_Should_Throw_ValidationException_When_Data_Is_Invalid()
    {
        var repository = new FakeSalesRepository();
        var publisher = new FakeEventPublisher();
        var service = new SalesService(repository, publisher);
        var sale = CreateInvalidSale();

        var ex = Assert.Throws<ValidationException>(() => service.Create(sale));

        Assert.NotEmpty(ex.Errors);
        Assert.Equal("PENDING", sale.Status);
        Assert.Equal(0, publisher.CallCount);
        Assert.Empty(repository.CreatedSales);
        Assert.Empty(repository.CreatedDetailsCalls);
    }

    [Fact]
    public void Create_Should_Generate_Only_Sale_Id_When_Detail_Id_Exists_And_Subtotal_Is_Zero()
    {
        var repository = new FakeSalesRepository();
        var publisher = new FakeEventPublisher();
        var service = new SalesService(repository, publisher);
        var detailId = Guid.NewGuid();
        var sale = CreateValidSale(Guid.Empty, new List<SaleDetail>
        {
            CreateDetail(detailId, Guid.Empty, 3, 20m, 0m)
        });

        service.Create(sale);

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal(detailId, sale.Details[0].Id);
        Assert.Equal(sale.Id, sale.Details[0].SaleId);
        Assert.Equal(60m, sale.Details[0].Subtotal);
    }

    [Fact]
    public void Create_Should_Generate_Only_Detail_Id_When_Sale_Id_Exists_And_Subtotal_Is_Present()
    {
        var repository = new FakeSalesRepository();
        var publisher = new FakeEventPublisher();
        var service = new SalesService(repository, publisher);
        var saleId = Guid.NewGuid();
        var sale = CreateValidSale(saleId, new List<SaleDetail>
        {
            CreateDetail(Guid.Empty, Guid.Empty, 4, 10m, 40m)
        });

        service.Create(sale);

        Assert.Equal(saleId, sale.Id);
        Assert.NotEqual(Guid.Empty, sale.Details[0].Id);
        Assert.Equal(saleId, sale.Details[0].SaleId);
        Assert.Equal(40m, sale.Details[0].Subtotal);
    }

    [Fact]
    public void Create_Should_Process_Multiple_Details_And_Set_Correct_Relations()
    {
        var repository = new FakeSalesRepository();
        var publisher = new FakeEventPublisher();
        var service = new SalesService(repository, publisher);
        var sale = CreateValidSale(Guid.Empty, new List<SaleDetail>
        {
            CreateDetail(Guid.Empty, Guid.Empty, 1, 10m, 0m),
            CreateDetail(Guid.Empty, Guid.Empty, 2, 5m, 0m),
            CreateDetail(Guid.Empty, Guid.Empty, 3, 7m, 0m)
        });

        service.Create(sale);

        Assert.Equal(3, sale.Details.Count);
        Assert.All(sale.Details, detail => Assert.Equal(sale.Id, detail.SaleId));
        Assert.Equal(new[] { 10m, 10m, 21m }, sale.Details.Select(d => d.Subtotal).ToArray());
        Assert.Equal("sales.pending", publisher.RoutingKey);
        Assert.Equal(1, publisher.CallCount);
    }

    private static Sale CreateValidSale(Guid saleId, List<SaleDetail> details)
    {
        return new Sale
        {
            Id = saleId,
            ClientId = Guid.NewGuid(),
            ClientName = "Cliente",
            ClientCi = "123456",
            UserId = Guid.NewGuid(),
            UserName = "Usuario",
            SaleDate = DateTimeOffset.UtcNow,
            Subtotal = 100m,
            Total = 100m,
            Status = "ignored",
            Details = details
        };
    }

    private static Sale CreateInvalidSale()
    {
        return new Sale
        {
            Id = Guid.Empty,
            ClientId = Guid.Empty,
            UserId = Guid.Empty,
            SaleDate = default,
            Subtotal = -1m,
            Total = -2m,
            Status = "INVALID_STATUS",
            CreatedAt = default,
            Details = new List<SaleDetail>
            {
                new SaleDetail
                {
                    Id = Guid.Empty,
                    SaleId = Guid.Empty,
                    ProductId = Guid.Empty,
                    Quantity = 0,
                    UnitPrice = -1m,
                    Subtotal = -1m
                }
            }
        };
    }

    private static SaleDetail CreateDetail(Guid id, Guid saleId, int quantity, decimal unitPrice, decimal subtotal)
    {
        return new SaleDetail
        {
            Id = id,
            SaleId = saleId,
            ProductId = Guid.NewGuid(),
            ProductName = "Producto",
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal
        };
    }

    private sealed class FakeSalesRepository : ISalesRepository
    {
        public List<Sale> CreatedSales { get; } = new();
        public List<(Guid saleId, List<SaleDetail> details)> CreatedDetailsCalls { get; } = new();

        public List<Sale> GetAll() => [];
        public Sale? Read(Guid id) => null;
        public List<SaleDetail> GetDetails(Guid saleId) => [];
        public void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details) => CreatedDetailsCalls.Add((saleId, details.ToList()));
        public void Create(Sale sale) => CreatedSales.Add(sale);
        public void Update(Sale sale) { }
        public void Delete(Guid id) { }
    }

    private sealed class FakeEventPublisher : IEventPublisher
    {
        public int CallCount { get; private set; }
        public string? RoutingKey { get; private set; }
        public object? Event { get; private set; }
        public List<object> LastProducts { get; private set; } = new();

        public Task PublishAsync(string routingKey, object @event)
        {
            CallCount++;
            RoutingKey = routingKey;
            Event = @event;
            LastProducts = ReadProducts(@event);
            return Task.CompletedTask;
        }

        private static List<object> ReadProducts(object @event)
        {
            var property = @event.GetType().GetProperty("Products");
            var value = property?.GetValue(@event) as IEnumerable<object>;
            return value?.ToList() ?? new List<object>();
        }
    }
}

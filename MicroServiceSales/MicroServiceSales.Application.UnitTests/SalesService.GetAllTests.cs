using MicroServiceSales.Application.Services;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Application.UnitTests;

public class SalesServiceGetAllTests
{
	[Fact]
	public void GetAll_Should_Return_Mapped_Sales_When_Repository_Has_At_Least_One_Record()
	{
		var expected = new List<Sale>
		{
			new()
			{
				Id = Guid.NewGuid(),
				ClientId = Guid.NewGuid(),
				UserId = Guid.NewGuid(),
				SaleDate = DateTimeOffset.UtcNow,
				Subtotal = 100m,
				Total = 100m,
				Status = "PENDING",
				CreatedAt = DateTimeOffset.UtcNow
			}
		};

		var repository = new FakeSalesRepository(expected);
		var service = new SalesService(repository, new NullEventPublisher());

		var result = service.GetAll();

		Assert.Equal(1, repository.GetAllCalls);
		Assert.NotNull(result);
		Assert.Single(result);
		Assert.Equal(expected[0].Id, result[0].Id);
		Assert.Equal(expected[0].ClientId, result[0].ClientId);
		Assert.Equal(expected[0].Status, result[0].Status);
	}

	[Fact]
	public void GetAll_Should_Return_Empty_List_When_Repository_Returns_No_Records()
	{
		var repository = new FakeSalesRepository([]);
		var service = new SalesService(repository, new NullEventPublisher());

		var result = service.GetAll();

		Assert.Equal(1, repository.GetAllCalls);
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	private sealed class FakeSalesRepository(List<Sale> salesToReturn) : ISalesRepository
	{
		public int GetAllCalls { get; private set; }

		public List<Sale> GetAll()
		{
			GetAllCalls++;
			return salesToReturn;
		}

		public Sale? Read(Guid id) => null;
		public List<SaleDetail> GetDetails(Guid saleId) => [];
		public void CreateDetails(Guid saleId, IEnumerable<SaleDetail> details) { }
		public void Create(Sale sale) { }
		public void Update(Sale sale) { }
		public void Delete(Guid id) { }
	}

	private sealed class NullEventPublisher : IEventPublisher
	{
		public Task PublishAsync(string routingKey, object @event) => Task.CompletedTask;
	}
}

using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class RabbitConsumerForSalesOnReceivedTests
{
    [Fact]
    public async Task ProcessMessageAsync_Should_Save_Completed_And_Publish_Confirmed_When_Status_Is_Approved()
    {
        var saleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var json = $$"""
        {
          "SaleId": "{{saleId}}",
          "Status": "APPROVED",
          "UserId": "{{userId}}",
          "UserName": "Carlos",
          "ClientId": "{{clientId}}",
          "ClientName": "Ana",
          "ClientCi": "123456",
          "Subtotal": 100.0,
          "Total": 100.0,
          "SaleDate": "2026-04-20T12:00:00Z",
          "Products": [
            {
              "ProductId": "{{Guid.NewGuid()}}",
              "ProductName": "Book A",
              "Quantity": 2,
              "UnitPrice": 25.0
            },
            {
              "ProductId": "{{Guid.NewGuid()}}",
              "ProductName": "Book B",
              "Quantity": 1,
              "UnitPrice": 50.0
            }
          ]
        }
        """;

        var repoMock = new Mock<ISalesRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.Create(It.Is<Sale>(s => s.Id == saleId && s.Status == "COMPLETED")));
        repoMock.Setup(r => r.CreateDetails(saleId, It.Is<IEnumerable<SaleDetail>>(d => d.Count() == 2)));

        var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
        publisherMock.Setup(p => p.PublishAsync("sales.confirmed", It.IsAny<object>())).Returns(Task.CompletedTask);

        var sut = CreateSut(repoMock.Object, publisherMock.Object, out _, out _);

        await sut.ProcessMessageAsync(json);

        repoMock.Verify(r => r.Create(It.IsAny<Sale>()), Times.Once);
        repoMock.Verify(r => r.CreateDetails(saleId, It.IsAny<IEnumerable<SaleDetail>>()), Times.Once);
        publisherMock.Verify(p => p.PublishAsync("sales.confirmed", It.IsAny<object>()), Times.Once);
        publisherMock.Verify(p => p.PublishAsync("sales.rejected", It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_Should_Save_Cancelled_And_Publish_Rejected_When_Status_Is_Rejected()
    {
        var saleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var json = $$"""
        {
          "SaleId": "{{saleId}}",
          "Status": "REJECTED",
          "Error": "No stock",
          "UserId": "{{userId}}",
          "UserName": "Carlos",
          "ClientId": "{{clientId}}",
          "ClientName": "Ana",
          "ClientCi": "123456",
          "Subtotal": 100.0,
          "Total": 100.0,
          "SaleDate": "2026-04-20T12:00:00Z",
          "Products": []
        }
        """;

        var repoMock = new Mock<ISalesRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.Create(It.Is<Sale>(s => s.Id == saleId && s.Status == "CANCELLED" && s.CancellationReason == "No stock")));

        var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
        publisherMock.Setup(p => p.PublishAsync("sales.rejected", It.IsAny<object>())).Returns(Task.CompletedTask);

        var sut = CreateSut(repoMock.Object, publisherMock.Object, out _, out _);

        await sut.ProcessMessageAsync(json);

        repoMock.Verify(r => r.Create(It.IsAny<Sale>()), Times.Once);
        repoMock.Verify(r => r.CreateDetails(It.IsAny<Guid>(), It.IsAny<IEnumerable<SaleDetail>>()), Times.Never);
        publisherMock.Verify(p => p.PublishAsync("sales.rejected", It.IsAny<object>()), Times.Once);
        publisherMock.Verify(p => p.PublishAsync("sales.confirmed", It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_Should_Do_Nothing_When_Status_Is_Unknown()
    {
        var json = $$"""
        {
          "SaleId": "{{Guid.NewGuid()}}",
          "Status": "UNKNOWN",
          "UserId": "{{Guid.NewGuid()}}",
          "ClientId": "{{Guid.NewGuid()}}",
          "Subtotal": 100.0,
          "Total": 100.0,
          "SaleDate": "2026-04-20T12:00:00Z",
          "Products": []
        }
        """;

        var repoMock = new Mock<ISalesRepository>(MockBehavior.Strict);
        var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);

        var sut = CreateSut(repoMock.Object, publisherMock.Object, out _, out _);

        await sut.ProcessMessageAsync(json);

        repoMock.Verify(r => r.Create(It.IsAny<Sale>()), Times.Never);
        repoMock.Verify(r => r.CreateDetails(It.IsAny<Guid>(), It.IsAny<IEnumerable<SaleDetail>>()), Times.Never);
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_Should_Log_Error_And_Skip_Processing_When_Json_Is_Invalid()
    {
        const string json = "{ malformed json";

        var repoMock = new Mock<ISalesRepository>(MockBehavior.Strict);
        var publisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);

        var sut = CreateSut(repoMock.Object, publisherMock.Object, out var loggerMock, out _);

        await sut.ProcessMessageAsync(json);

        repoMock.Verify(r => r.Create(It.IsAny<Sale>()), Times.Never);
        repoMock.Verify(r => r.CreateDetails(It.IsAny<Guid>(), It.IsAny<IEnumerable<SaleDetail>>()), Times.Never);
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private static RabbitConsumerForSales CreateSut(
        ISalesRepository repository,
        IEventPublisher publisher,
        out Mock<ILogger<RabbitConsumerForSales>> loggerMock,
        out Mock<IServiceScopeFactory> scopeFactoryMock)
    {
        var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProviderMock.Setup(s => s.GetService(typeof(ISalesRepository))).Returns(repository);
        serviceProviderMock.Setup(s => s.GetService(typeof(IEventPublisher))).Returns(publisher);

        var scopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        scopeMock.Setup(s => s.Dispose());

        scopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        loggerMock = new Mock<ILogger<RabbitConsumerForSales>>();

        var connMock = new Mock<IConnection>(MockBehavior.Loose);
        var channelMock = new Mock<IModel>(MockBehavior.Loose);

        return new RabbitConsumerForSales(
            scopeFactoryMock.Object,
            loggerMock.Object,
            connMock.Object,
            channelMock.Object);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using System.Text.Json;
using Xunit;
using MicroServiceProduct.Infraestructure.Messaging;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Application.Services;
using RabbitMQ.Client.Events;

namespace MicroServiceProduct.Infraestucture.test
{
    public class RabbitConsumerForProductTest
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<RabbitConsumerForProduct>> _mockLog;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IConnection> _mockConn;
        private readonly Mock<IModel> _mockChannel;

        public RabbitConsumerForProductTest()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockLog = new Mock<ILogger<RabbitConsumerForProduct>>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockConn = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();

            _mockConfig.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
            _mockConfig.Setup(c => c["RabbitMQ:Exchange"]).Returns("saga.exchange");
            _mockConn.Setup(x => x.CreateModel()).Returns(_mockChannel.Object);
        }

        [Fact]
        public async Task TC_C1_OnReceived_WhenStockAvailable_ShouldPublishApproved()
        {
            // --- ARRANGE ---
            var (consumer, mocks) = SetupConsumerWithMocks();

            // Simulamos que TryReserveStock es EXITOSO
            string error;
            mocks.ProductService.Setup(x => x.TryReserveStock(It.IsAny<Dictionary<Guid, int>>(), out error))
                .Returns(true);

            var json = CreateValidJson();
            var body = Encoding.UTF8.GetBytes(json);
            var args = new BasicDeliverEventArgs { Body = body };

            // --- ACT ---
            // Invocamos el método (asumiendo que es internal)
            await consumer.OnReceived(this, args);

            // --- ASSERT ---
            // Verificamos que se publicó el evento de APROBADO
            mocks.Publisher.Verify(x => x.PublishAsync("sales.approved", It.Is<object>(obj =>
                obj.ToString().Contains("APPROVED"))), Times.Once);
        }

        [Fact]
        public async Task TC_C2_OnReceived_WhenNoStock_ShouldPublishRejected()
        {
            // --- ARRANGE ---
            var (consumer, mocks) = SetupConsumerWithMocks();

            // Simulamos que TryReserveStock FALLA
            string error = "No hay stock";
            mocks.ProductService.Setup(x => x.TryReserveStock(It.IsAny<Dictionary<Guid, int>>(), out error))
                .Returns(false);

            var json = CreateValidJson();
            var args = new BasicDeliverEventArgs { Body = Encoding.UTF8.GetBytes(json) };

            // --- ACT ---
            await consumer.OnReceived(this, args);

            // --- ASSERT ---
            // Verificamos que se publicó el evento de RECHAZADO
            mocks.Publisher.Verify(x => x.PublishAsync("sales.approved", It.Is<object>(obj =>
                obj.ToString().Contains("REJECTED"))), Times.Once);
        }

        [Fact]
        public async Task TC_C3_OnReceived_WhenJsonInvalid_ShouldLogError()
        {
            // --- ARRANGE ---
            var consumer = new RabbitConsumerForProduct(_mockConfig.Object, _mockScopeFactory.Object, _mockLog.Object, _mockConn.Object);
            var args = new BasicDeliverEventArgs { Body = Encoding.UTF8.GetBytes("{ invalid json }") };

            // --- ACT ---
            await consumer.OnReceived(this, args);

            // --- ASSERT ---
            // Verificamos que se logueó el error y no se publicó nada
            _mockLog.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        // --- HELPERS PARA MOCKS ---
        private (RabbitConsumerForProduct, InternalMocks) SetupConsumerWithMocks()
        {
            var mocks = new InternalMocks();
            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

            mockServiceProvider.Setup(x => x.GetService(typeof(IProductService))).Returns(mocks.ProductService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IProductRepository))).Returns(mocks.ProductRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IEventPublisher))).Returns(mocks.Publisher.Object);

            var consumer = new RabbitConsumerForProduct(_mockConfig.Object, _mockScopeFactory.Object, _mockLog.Object, _mockConn.Object);
            return (consumer, mocks);
        }

        private string CreateValidJson()
        {
            return JsonSerializer.Serialize(new
            {
                SaleId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                Subtotal = 100.0,
                Total = 110.0,
                SaleDate = DateTimeOffset.Now,
                Products = new[] { new { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 50.0 } }
            });
        }

        private class InternalMocks
        {
            public Mock<IProductService> ProductService { get; } = new Mock<IProductService>();
            public Mock<IProductRepository> ProductRepo { get; } = new Mock<IProductRepository>();
            public Mock<IEventPublisher> Publisher { get; } = new Mock<IEventPublisher>();
        }
    }
}

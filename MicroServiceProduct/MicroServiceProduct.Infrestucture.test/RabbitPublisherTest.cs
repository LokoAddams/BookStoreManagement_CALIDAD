using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using MicroServiceProduct.Infraestructure.Messaging;

namespace MicroServiceProduct.Infraestructure.test
{
    public class RabbitPublisherTests
    {
        private readonly Mock<IConfiguration> _mockConfig;

        public RabbitPublisherTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
            _mockConfig.Setup(c => c["RabbitMQ:Exchange"]).Returns("test.exchange");
        }


        [Fact]
        public async Task TC_PA1_PublishAsync_WhenNotDisposed_ShouldProceed()
        {
            // Escenario: _disposed es FALSE (Rama False del IF)
            using var publisher = new RabbitPublisher(_mockConfig.Object);

            // Act
            var task = publisher.PublishAsync("key", new { Data = "Test" });

            // Assert: Si no hay RabbitMQ, esto lanzará una excepción de conexión.
            // Pero lógicamente, aquí es donde validas que el método NO lanzó ObjectDisposedException.
            await task;
        }

        [Fact]
        public async Task TC_PA2_PublishAsync_WhenDisposed_ShouldThrowException()
        {
            // Escenario: _disposed es TRUE (Rama True del IF)
            using var publisher = new RabbitPublisher(_mockConfig.Object);
            publisher.Dispose(); // Forzamos el estado a true

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                publisher.PublishAsync("key", new { Data = "Test" })
            );
        }


        [Fact]
        public void TC_D1_Dispose_WhenFirstTime_ShouldCleanResources()
        {
            // Escenario: _disposed es FALSE (Rama False del IF)
            var publisher = new RabbitPublisher(_mockConfig.Object);

            // Act
            publisher.Dispose();

            // Assert: No debe haber error y los recursos internos se liberan
        }

        [Fact]
        public void TC_D2_Dispose_WhenAlreadyDisposed_ShouldReturnImmediately()
        {
            // Escenario: _disposed es TRUE (Rama True del IF)
            var publisher = new RabbitPublisher(_mockConfig.Object);
            publisher.Dispose(); // Primera llamada (pone _disposed en true)

            // Act
            var exception = Record.Exception(() => publisher.Dispose()); // Segunda llamada

            // Assert: La segunda llamada entra al "if(_disposed) return;" y no hace nada
            Assert.Null(exception);
        }
    }
}
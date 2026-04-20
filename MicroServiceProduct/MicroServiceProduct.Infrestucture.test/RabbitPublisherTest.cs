using MicroServiceProduct.Infraestructure.Messaging;
using Microsoft.Extensions.Configuration;
using Moq;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MicroServiceProduct.Infraestructure.test
{
    public class RabbitPublisherTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IConnection> _mockConn;
        private readonly Mock<IModel> _mockChannel;

        public RabbitPublisherTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConn = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();

            var mockProps = new Mock<IBasicProperties>();

            _mockConfig.Setup(c => c["RabbitMQ:Host"]).Returns("localhost");
            _mockConfig.Setup(c => c["RabbitMQ:Exchange"]).Returns("test.exchange");

            _mockConn.Setup(x => x.CreateModel()).Returns(_mockChannel.Object);

            _mockChannel.Setup(x => x.CreateBasicProperties()).Returns(mockProps.Object);
        }

        [Fact]
        public async Task TC_PA1_PublishAsync_WhenNotDisposed_ShouldProceed()
        {
            // Arrange
            using var publisher = new RabbitPublisher(_mockConfig.Object, _mockConn.Object);

            // Act
            await publisher.PublishAsync("key", new { Data = "Test" });

            // Assert: Usamos el método base de la interfaz que incluye el bool 'mandatory'
            _mockChannel.Verify(x => x.BasicPublish(
                It.IsAny<string>(),             // exchange
                "key",                          // routingKey
                It.IsAny<bool>(),               // mandatory (ESTE ES EL QUE FALTABA)
                It.IsAny<IBasicProperties>(),   // basicProperties
                It.IsAny<ReadOnlyMemory<byte>>() // body
            ), Times.Once);
        }

        [Fact]
        public async Task TC_PA2_PublishAsync_WhenDisposed_ShouldThrowException()
        {
            // Escenario: _disposed es TRUE (Rama True)
            using var publisher = new RabbitPublisher(_mockConfig.Object, _mockConn.Object);

            publisher.Dispose(); // Forzamos el estado a true

            // Act & Assert: Debe lanzar la excepción sin tocar la red
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                publisher.PublishAsync("key", new { Msg = "Test" })
            );
        }

        [Fact]
        public void TC_D1_Dispose_WhenFirstTime_ShouldCleanResources()
        {
            // Escenario: _disposed es FALSE (Rama False)
            var publisher = new RabbitPublisher(_mockConfig.Object, _mockConn.Object);

            // Act
            publisher.Dispose();

            // Assert: Verificamos que se llamó al Dispose de los recursos internos
            _mockChannel.Verify(x => x.Dispose(), Times.Once);
            _mockConn.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void TC_D2_Dispose_WhenAlreadyDisposed_ShouldReturnImmediately()
        {
            // Escenario: _disposed es TRUE (Rama True)
            var publisher = new RabbitPublisher(_mockConfig.Object, _mockConn.Object);

            publisher.Dispose(); // Primera llamada (limpia recursos)

            // Act
            var exception = Record.Exception(() => publisher.Dispose()); // Segunda llamada

            // Assert: La segunda llamada no debe intentar limpiar de nuevo ni fallar
            Assert.Null(exception);
            _mockChannel.Verify(x => x.Dispose(), Times.Once); // Solo se debió llamar una vez
        }
    }
}
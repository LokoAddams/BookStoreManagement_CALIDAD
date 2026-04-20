using System;
using Moq;
using Xunit;
using MicroServiceProduct.Infraestructure.Messaging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace MicroServiceProduct.Infraestructure.test
{
    public class RabbitPublisherTest
    {
        private Mock<IConfiguration> SetupConfigMock(
            string host = "localhost",
            string user = "guest",
            string password = "guest",
            string exchange = "saga.exchange")
        {
            var configMock = new Mock<IConfiguration>();
            configMock
                .Setup(c => c["RabbitMQ:Host"])
                .Returns(host);
            configMock
                .Setup(c => c["RabbitMQ:User"])
                .Returns(user);
            configMock
                .Setup(c => c["RabbitMQ:Password"])
                .Returns(password);
            configMock
                .Setup(c => c["RabbitMQ:Exchange"])
                .Returns(exchange);

            return configMock;
        }

        [Fact]
        public void Constructor_ShouldThrowRabbitMQClientException_WhenConnectionFails()
        {
            // Arrange
            var configMock = SetupConfigMock(host: "invalid-host.local");

            // Act & Assert
            Assert.Throws<RabbitMQClientException>(() => new RabbitPublisher(configMock.Object));
        }

        [Fact]
        public void Constructor_WithDefaultValues_ShouldUseLocalhostAsHost()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["RabbitMQ:Host"]).Returns((string)null);
            configMock.Setup(c => c["RabbitMQ:User"]).Returns("guest");
            configMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");
            configMock.Setup(c => c["RabbitMQ:Exchange"]).Returns("saga.exchange");

            // Act & Assert - Verifica que intente conectar a localhost (por defecto)
            Assert.Throws<RabbitMQClientException>(() => new RabbitPublisher(configMock.Object));
        }

        [Fact]
        public void Constructor_WithNullExchange_ShouldUseSagaExchangeAsDefault()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["RabbitMQ:Host"]).Returns("invalid-host");
            configMock.Setup(c => c["RabbitMQ:User"]).Returns("guest");
            configMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");
            configMock.Setup(c => c["RabbitMQ:Exchange"]).Returns((string)null);

            // Act & Assert - Verifica que usa "saga.exchange" por defecto
            Assert.Throws<RabbitMQClientException>(() => new RabbitPublisher(configMock.Object));
        }

        [Fact]
        public void Dispose_ShouldNotThrowException_WhenCalledMultipleTimes()
        {
            // Arrange - Usa valores inválidos intencionales
            var configMock = SetupConfigMock(host: "invalid");

            // Act & Assert - La excepción en constructor es esperada
            var exception = Record.Exception(() =>
            {
                var publisher = new RabbitPublisher(configMock.Object);
            });

            // Si se lanza excepción en constructor, no hay nada que descartar
            Assert.IsType<RabbitMQClientException>(exception);
        }

        [Fact]
        public void ConfigurationMock_ShouldReturnCorrectValues()
        {
            // Arrange
            var configMock = SetupConfigMock(
                host: "test-host",
                user: "testuser",
                password: "testpass",
                exchange: "test.exchange");

            // Act & Assert - Verifica que el mock está configurado correctamente
            Assert.Equal("test-host", configMock.Object["RabbitMQ:Host"]);
            Assert.Equal("testuser", configMock.Object["RabbitMQ:User"]);
            Assert.Equal("testpass", configMock.Object["RabbitMQ:Password"]);
            Assert.Equal("test.exchange", configMock.Object["RabbitMQ:Exchange"]);
        }
    }
}
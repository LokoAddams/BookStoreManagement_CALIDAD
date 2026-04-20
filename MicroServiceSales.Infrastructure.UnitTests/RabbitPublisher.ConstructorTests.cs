using Microsoft.Extensions.Configuration;
using MicroServiceSales.Infrastructure.Messaging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class RabbitPublisherConstructorTests
{
    private static Mock<IConfiguration> SetupConfigMock(
        string? host = "localhost",
        string? user = "guest",
        string? password = "guest",
        string? exchange = "saga.exchange")
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["RabbitMQ:Host"]).Returns(host);
        configMock.Setup(c => c["RabbitMQ:User"]).Returns(user);
        configMock.Setup(c => c["RabbitMQ:Password"]).Returns(password);
        configMock.Setup(c => c["RabbitMQ:Exchange"]).Returns(exchange);

        return configMock;
    }

    [Fact]
    public void Constructor_Should_Throw_BrokerUnreachableException_When_Connection_Fails_With_Complete_Configuration()
    {
        var configMock = SetupConfigMock(
            host: "invalid-host.local",
            user: "company-user",
            password: "company-pass",
            exchange: "company.exchange");

        Assert.Throws<BrokerUnreachableException>(() => new RabbitPublisher(configMock.Object));
    }

    [Fact]
    public void Constructor_Should_Throw_BrokerUnreachableException_When_Connection_Fails_With_Null_Configuration()
    {
        var configMock = SetupConfigMock(
            host: null,
            user: null,
            password: null,
            exchange: null);

        Assert.Throws<BrokerUnreachableException>(() => new RabbitPublisher(configMock.Object));
    }
}
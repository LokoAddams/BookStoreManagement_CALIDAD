using MicroServiceSales.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class RabbitConsumerForSalesExecuteAsyncTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Register_Consumer_And_Start_Listening()
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<RabbitConsumerForSales>>();
        var connMock = new Mock<IConnection>(MockBehavior.Loose);
        var channelMock = new Mock<IModel>(MockBehavior.Loose);

        var sut = new TestableRabbitConsumerForSales(
            scopeFactoryMock.Object,
            loggerMock.Object,
            connMock.Object,
            channelMock.Object);

        await sut.InvokeExecuteAsync(CancellationToken.None);

        var basicConsumeInvocation = channelMock.Invocations
            .FirstOrDefault(i => i.Method.Name == nameof(IModel.BasicConsume));

        Assert.NotNull(basicConsumeInvocation);
        Assert.Contains(basicConsumeInvocation.Arguments, a => a is string q && q == "sales.queue");
        Assert.Contains(basicConsumeInvocation.Arguments, a => a is bool autoAck && autoAck);
        Assert.Contains(basicConsumeInvocation.Arguments, a => a is AsyncEventingBasicConsumer);
    }

    private sealed class TestableRabbitConsumerForSales : RabbitConsumerForSales
    {
        public TestableRabbitConsumerForSales(
            IServiceScopeFactory scopeFactory,
            ILogger<RabbitConsumerForSales> log,
            IConnection connection,
            IModel channel)
            : base(scopeFactory, log, connection, channel)
        {
        }

        public Task InvokeExecuteAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);
        }
    }
}

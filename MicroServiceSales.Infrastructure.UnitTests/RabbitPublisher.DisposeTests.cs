using MicroServiceSales.Infrastructure.Messaging;
using Moq;
using RabbitMQ.Client;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class RabbitPublisherDisposeTests
{
    [Fact]
    public void Dispose_Should_Close_Channel_And_Connection_On_First_Call()
    {
        var connMock = new Mock<IConnection>(MockBehavior.Strict);
        var channelMock = new Mock<IModel>(MockBehavior.Strict);

        channelMock.Setup(c => c.Dispose());
        connMock.Setup(c => c.Dispose());

        var sut = new RabbitPublisher(connMock.Object, channelMock.Object, "saga.exchange");

        sut.Dispose();

        channelMock.Verify(c => c.Dispose(), Times.Once);
        connMock.Verify(c => c.Dispose(), Times.Once);
        Assert.Throws<ObjectDisposedException>(() => sut.PublishAsync("rk", new { }).GetAwaiter().GetResult());
    }

    [Fact]
    public void Dispose_Should_Return_Immediately_On_Redundant_Call()
    {
        var connMock = new Mock<IConnection>(MockBehavior.Strict);
        var channelMock = new Mock<IModel>(MockBehavior.Strict);

        channelMock.Setup(c => c.Dispose());
        connMock.Setup(c => c.Dispose());

        var sut = new RabbitPublisher(connMock.Object, channelMock.Object, "saga.exchange");

        sut.Dispose();
        sut.Dispose();

        channelMock.Verify(c => c.Dispose(), Times.Once);
        connMock.Verify(c => c.Dispose(), Times.Once);
    }
}

using System.Text.Json;
using MicroServiceSales.Infrastructure.Messaging;
using Moq;
using RabbitMQ.Client;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class RabbitPublisherPublishAsyncTests
{
    private sealed class PublishEvent
    {
        public string Name { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }

    [Fact]
    public async Task PublishAsync_Should_Publish_Message_When_Not_Disposed()
    {
        var connMock = new Mock<IConnection>(MockBehavior.Strict);
        var channelMock = new Mock<IModel>(MockBehavior.Strict);
        var propertiesMock = new Mock<IBasicProperties>(MockBehavior.Strict);

        byte[]? publishedBody = null;
        string? publishedExchange = null;
        string? publishedRoutingKey = null;

        propertiesMock.SetupSet(p => p.DeliveryMode = 2);
        channelMock.Setup(c => c.CreateBasicProperties()).Returns(propertiesMock.Object);
        channelMock
            .Setup(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()))
            .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>((exchange, routingKey, _, _, body) =>
            {
                publishedExchange = exchange;
                publishedRoutingKey = routingKey;
                publishedBody = body.ToArray();
            });

        var sut = new RabbitPublisher(connMock.Object, channelMock.Object, "saga.exchange");
        var routingKey = "test";
        var @event = new PublishEvent { Name = "book", Quantity = 2 };
        var expectedBody = JsonSerializer.SerializeToUtf8Bytes(@event);

        await sut.PublishAsync(routingKey, @event);

        channelMock.Verify(c => c.CreateBasicProperties(), Times.Once);
        propertiesMock.VerifySet(p => p.DeliveryMode = 2, Times.Once);
        channelMock.Verify(c => c.BasicPublish("saga.exchange", routingKey, false, propertiesMock.Object, It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
        Assert.Equal("saga.exchange", publishedExchange);
        Assert.Equal(routingKey, publishedRoutingKey);
        Assert.NotNull(publishedBody);
        Assert.Equal(expectedBody, publishedBody);
    }

    [Fact]
    public async Task PublishAsync_Should_Throw_ObjectDisposedException_When_Disposed()
    {
        var connMock = new Mock<IConnection>(MockBehavior.Strict);
        var channelMock = new Mock<IModel>(MockBehavior.Strict);

        channelMock.Setup(c => c.Dispose());
        connMock.Setup(c => c.Dispose());

        var sut = new RabbitPublisher(connMock.Object, channelMock.Object, "saga.exchange");
        sut.Dispose();

        var ex = await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.PublishAsync("test", new { Value = 1 }));

        Assert.Equal(nameof(RabbitPublisher), ex.ObjectName);
        channelMock.Verify(c => c.CreateBasicProperties(), Times.Never);
        Assert.DoesNotContain(channelMock.Invocations, invocation => invocation.Method.Name == nameof(IModel.BasicPublish));
    }
}

using MicroServiceSales.Domain.Interfaces;

namespace MicroServiceSales.Application.UnitTests;

internal sealed class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync(string routingKey, object @event) => Task.CompletedTask;
}
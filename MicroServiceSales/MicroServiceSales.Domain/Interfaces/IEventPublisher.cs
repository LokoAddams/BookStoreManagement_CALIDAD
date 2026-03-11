using System.Threading.Tasks;

namespace MicroServiceSales.Domain.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync(string routingKey, object @event);
    }
}

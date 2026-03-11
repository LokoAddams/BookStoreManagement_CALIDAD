using System.Threading.Tasks;

namespace MicroServiceProduct.Domain.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync(string routingKey, object @event);
    }
}

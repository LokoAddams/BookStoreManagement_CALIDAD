using System;
using System.Text.Json;
using System.Threading.Tasks;
using MicroServiceProduct.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace MicroServiceProduct.Infraestructure.Messaging
{
    public sealed class RabbitPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection _conn;
        private readonly IModel _channel;
        private readonly string _exchange;
        private bool _disposed;

        public RabbitPublisher(IConfiguration cfg)
        {
            var factory = new ConnectionFactory
            {
                HostName = cfg["RabbitMQ:Host"] ?? "localhost",
                UserName = cfg["RabbitMQ:User"] ?? "guest",
                Password = cfg["RabbitMQ:Password"] ?? "guest",
                DispatchConsumersAsync = true
            };
            _exchange = cfg["RabbitMQ:Exchange"] ?? "saga.exchange";
            _conn = factory.CreateConnection();
            _channel = _conn.CreateModel();
            _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);
        }

        public Task PublishAsync(string routingKey, object @event)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RabbitPublisher));
            }

            var body = JsonSerializer.SerializeToUtf8Bytes(@event);
            var props = _channel.CreateBasicProperties();
            props.DeliveryMode = 2; // persistent
            _channel.BasicPublish(_exchange, routingKey, props, body);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _channel?.Dispose();
            _conn?.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using MicroServiceReports.Domain.Models;
using MicroServiceReports.Domain.Ports;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroServiceReports.Infraestructure.Rabbit
{
    public class RabbitSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Exchange { get; set; } = "saga.exchange";
        public string RoutingKey { get; set; } = "sales.confirmed";
        public string Queue { get; set; } = "reports.queue";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }

    public class SalesConfirmedBackgroundService : BackgroundService
    {
        private readonly ILogger<SalesConfirmedBackgroundService> _logger;
        private readonly RabbitSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;
        private RabbitMQ.Client.IChannel? _channel;
        private RabbitMQ.Client.IConnection? _connection;

        public SalesConfirmedBackgroundService(ILogger<SalesConfirmedBackgroundService> logger, IOptions<RabbitSettings> options, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _settings = options.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnectAndStartConsumerAsync(stoppingToken);
        }

        private async Task ConnectAndStartConsumerAsync(CancellationToken stoppingToken)
        {
            var factory = new RabbitMQ.Client.ConnectionFactory()
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare queue/bindings is optional if already created, but harmless.
            await _channel.ExchangeDeclareAsync(_settings.Exchange, ExchangeType.Topic, durable: true);
            await _channel.QueueDeclareAsync(_settings.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueBindAsync(_settings.Queue, _settings.Exchange, _settings.RoutingKey);

            var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(_channel);
            // Subscribe to ReceivedAsync (async handler)
            consumer.ReceivedAsync += async (object sender, RabbitMQ.Client.Events.BasicDeliverEventArgs ea) =>
            {
                await HandleMessageAsync(ea).ConfigureAwait(false);
            };

            // BasicConsume(queue, autoAck, consumer)
            await _channel.BasicConsumeAsync(_settings.Queue, false, consumer);

            _logger.LogInformation("RabbitMQ consumer started on queue {Queue}", _settings.Queue);
        }

        private async Task HandleMessageAsync(RabbitMQ.Client.Events.BasicDeliverEventArgs ea)
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());

            // 1. Extracción y Validación
            string saleId = ExtractSaleId(payload);

            if (string.IsNullOrEmpty(saleId))
            {
                await TerminarConErrorPermanente(ea, "Missing or invalid saleId");
                return;
            }

            // 2. Procesamiento y Persistencia
            await ProcesarMensajeAsync(ea, payload, saleId);
        }

        private string ExtractSaleId(string payload)
        {
            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                if (root.TryGetProperty("SaleId", out var prop)) return prop.GetString() ?? "";
                if (root.TryGetProperty("saleId", out var propLower)) return GetJsonValueAsString(propLower);

                return "";
            }
            catch (JsonException)
            {
                return "";
            }
        }

        private string GetJsonValueAsString(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Number
                ? element.GetInt64().ToString()
                : element.GetString() ?? "";
        }

        private async Task ProcesarMensajeAsync(RabbitMQ.Client.Events.BasicDeliverEventArgs ea, string payload, string saleId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                await GuardarEnRepositorio(scope, ea, payload, saleId);

                await ConfirmarMensaje(ea.DeliveryTag);
                _logger.LogInformation("Message persisted and ACKed. SaleId={SaleId}", saleId);
            }
            catch (Exception ex)
            {
                await ReintentarMensaje(ea, saleId, ex);
            }
        }

        private async Task GuardarEnRepositorio(IServiceScope scope, RabbitMQ.Client.Events.BasicDeliverEventArgs ea, string payload, string saleId)
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISaleEventRepository>();
            var detailRepo = scope.ServiceProvider.GetRequiredService<ISaleDetailRepository>();

            var record = new SaleEventRecord
            {
                Id = Guid.NewGuid(),
                SaleId = saleId,
                Payload = payload,
                Exchange = ea.Exchange,
                RoutingKey = ea.RoutingKey,
                ReceivedAt = DateTime.UtcNow
            };

            await repo.SaveAsync(record).ConfigureAwait(false);
            await SaveSaleDetailsAsync(payload, saleId, detailRepo).ConfigureAwait(false);
        }

        private async Task ConfirmarMensaje(ulong deliveryTag)
        {
            if (_channel != null) await _channel.BasicAckAsync(deliveryTag, false);
        }

        private async Task TerminarConErrorPermanente(RabbitMQ.Client.Events.BasicDeliverEventArgs ea, string motivo)
        {
            _logger.LogWarning("{Motivo}, will nack and not requeue. Tag={Tag}", motivo, ea.DeliveryTag);
            if (_channel != null) await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
        }

        private async Task ReintentarMensaje(RabbitMQ.Client.Events.BasicDeliverEventArgs ea, string saleId, Exception ex)
        {
            _logger.LogError(ex, "Failed to persist. Requeuing. SaleId={SaleId}", saleId);
            if (_channel != null) await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
        }

        private async Task SaveSaleDetailsAsync(string payload, string saleId, ISaleDetailRepository detailRepo)
        {
            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                // Buscar la propiedad Products (puede ser "Products" o "products")
                JsonElement productsElement;
                if (!root.TryGetProperty("Products", out productsElement))
                {
                    if (!root.TryGetProperty("products", out productsElement))
                    {
                        _logger.LogWarning("No Products array found in message for SaleId={SaleId}", saleId);
                        return;
                    }
                }

                if (productsElement.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("Products is not an array for SaleId={SaleId}", saleId);
                    return;
                }

                var details = new List<SaleDetailRecord>();
                var now = DateTime.UtcNow;

                foreach (var product in productsElement.EnumerateArray())
                {
                    var detail = new SaleDetailRecord
                    {
                        Id = Guid.NewGuid(),
                        SaleId = saleId,
                        CreatedAt = now
                    };

                    // ProductId
                    if (product.TryGetProperty("ProductId", out var productIdProp))
                    {
                        detail.ProductId = productIdProp.GetString() ?? string.Empty;
                    }
                    else if (product.TryGetProperty("productId", out var productIdLower))
                    {
                        detail.ProductId = productIdLower.GetString() ?? string.Empty;
                    }

                    // ProductName / Name
                    if (product.TryGetProperty("Name", out var nameProp))
                    {
                        detail.ProductName = nameProp.GetString() ?? string.Empty;
                    }
                    else if (product.TryGetProperty("name", out var nameLower))
                    {
                        detail.ProductName = nameLower.GetString() ?? string.Empty;
                    }
                    else if (product.TryGetProperty("ProductName", out var productNameProp))
                    {
                        detail.ProductName = productNameProp.GetString() ?? string.Empty;
                    }
                    else if (product.TryGetProperty("productName", out var productNameLower))
                    {
                        detail.ProductName = productNameLower.GetString() ?? string.Empty;
                    }

                    // Quantity
                    if (product.TryGetProperty("Quantity", out var quantityProp))
                    {
                        detail.Quantity = quantityProp.GetInt32();
                    }
                    else if (product.TryGetProperty("quantity", out var quantityLower))
                    {
                        detail.Quantity = quantityLower.GetInt32();
                    }

                    // UnitPrice
                    if (product.TryGetProperty("UnitPrice", out var unitPriceProp))
                    {
                        detail.UnitPrice = unitPriceProp.GetDecimal();
                    }
                    else if (product.TryGetProperty("unitPrice", out var unitPriceLower))
                    {
                        detail.UnitPrice = unitPriceLower.GetDecimal();
                    }

                    // Calcular Subtotal
                    detail.Subtotal = detail.UnitPrice * detail.Quantity;

                    details.Add(detail);
                }

                if (details.Count > 0)
                {
                    await detailRepo.SaveManyAsync(details).ConfigureAwait(false);
                    _logger.LogInformation("Saved {Count} sale details for SaleId={SaleId}", details.Count, saleId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse and save sale details for SaleId={SaleId}", saleId);
                throw; // Re-throw para que el mensaje se reencole
            }
        }

        public override void Dispose()
        {
            try
            {
                // Prefer safe close on channel/connection; swallow exceptions but log
                try
                {
                    if (_channel != null)
                    {
                        try
                        {
                            _channel.CloseAsync().GetAwaiter().GetResult();
                        }
                        catch
                        {
                            // ignore close exceptions
                        }

                        _channel.DisposeAsync().AsTask().GetAwaiter().GetResult();
                    }
                }
                catch (Exception cex)
                {
                    _logger.LogWarning(cex, "Error while closing channel");
                }

                try
                {
                    if (_connection != null)
                    {
                        try
                        {
                            _connection.CloseAsync().GetAwaiter().GetResult();
                        }
                        catch
                        {
                            // ignore
                        }

                        _connection.DisposeAsync().AsTask().GetAwaiter().GetResult();
                    }
                }
                catch (Exception conex)
                {
                    _logger.LogWarning(conex, "Error while closing connection");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while disposing RabbitMQ connection/channel");
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}

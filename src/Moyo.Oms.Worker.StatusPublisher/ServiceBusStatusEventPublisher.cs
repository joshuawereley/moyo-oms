using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Options;

using Moyo.Oms.Application.Abstractions.Messaging;
using Moyo.Oms.Contracts;

namespace Moyo.Oms.Worker.StatusPublisher;

/// <summary>
/// Publishes order-status changes to a Service Bus topic as OrderStatusChanged messages.
/// </summary>

public sealed class ServiceBusStatusEventPublisher : IStatusEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusStatusEventPublisher(IOptions<ServiceBusOptions> options)
    {
        ServiceBusOptions serviceBusOptions = options.Value;
        _client = new ServiceBusClient(serviceBusOptions.ConnectionString);
        _sender = _client.CreateSender(serviceBusOptions.TopicName);
    }

    public async Task PublishAsync(
        StatusChangeNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        OrderStatusChanged payload = new()
        {
            ClientPortalOrderId = notification.ClientPortalOrderId,
            Status = notification.Status.ToString(),
            StatusNote = notification.StatusNote,
            ChangedAt = notification.ChangedAt,
        };

        ServiceBusMessage message = new(JsonSerializer.Serialize(payload))
        {
            MessageId = notification.MessageId,
            ContentType = "application/json",
        };

        await _sender.SendMessageAsync(message, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}

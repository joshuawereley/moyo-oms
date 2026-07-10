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
    private readonly ServiceBusSender _sender;

    public ServiceBusStatusEventPublisher(IOptions<ServiceBusOptions> options, ServiceBusClient client)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(client);

        _sender = client.CreateSender(options.Value.TopicName);
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
        // The client is a container-owned singleton; disposing it here would be a double dispose.
        await _sender.DisposeAsync();
    }
}

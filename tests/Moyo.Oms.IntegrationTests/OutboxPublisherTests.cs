using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Messaging;
using Moyo.Oms.Application.Orders;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence.Repositories;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

[Collection("Database")]
public sealed class OutboxPublisherTests
{
    private readonly SqlServerFixture _fixture;

    public OutboxPublisherTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PublishPending_SendsNotificationAndMarksRowPublished()
    {
        var seed = await SeedPendingOutboxRowAsync();
        var publisher = new RecordingStatusEventPublisher();

        int published;
        await using (var context = _fixture.CreateContext())
        {
            var useCase = new OutboxPublisher(
                new OutgoingStatusEventRepository(context),
                publisher,
                context);
            published = await useCase.PublishPendingAsync(100);
        }

        published.Should().BeGreaterThanOrEqualTo(1);

        var notification = publisher.Published.Single(n => n.MessageId == seed.MessageId);
        notification.ClientPortalOrderId.Should().Be(seed.ClientPortalOrderId);
        notification.Status.Should().Be(OrderStatus.InProgress);
        notification.StatusNote.Should().Be("Picked up");
        notification.ChangedAt.Should().BeCloseTo(seed.ChangedAt, TimeSpan.FromSeconds(1));

        await using (var context = _fixture.CreateContext())
        {
            var row = await context.OutgoingStatusEvents.SingleAsync(e => e.Id == seed.OutboxId);
            row.Status.Should().Be(PublishStatus.Published);
            row.PublishedAt.Should().NotBeNull();
        }
    }

    private async Task<(int OutboxId, string MessageId, string ClientPortalOrderId, DateTimeOffset ChangedAt)> SeedPendingOutboxRowAsync()
    {
        await using var context = _fixture.CreateContext();

        var externalSystem = new ExternalSystem("Client Portal", IntegrationType.ServiceBus);
        context.ExternalSystems.Add(externalSystem);
        await context.SaveChangesAsync();

        var incomingEvent = new IncomingOrderEvent(new IncomingOrderEventDetails
        {
            ExternalSystemId = externalSystem.Id,
            ServiceBusMessageId = Guid.NewGuid().ToString(),
            ClientPortalOrderId = "CPO-PUB-1",
        });
        var order = new CustomerOrder(incomingEvent, new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-PUB-1",
            ClientReference = "REF-1",
        });
        context.CustomerOrders.Add(order);
        await context.SaveChangesAsync();

        var history = new OrderStatusHistory(new OrderStatusChange
        {
            OrderId = order.Id,
            ChangedByVendorUserId = null,
            PreviousStatus = OrderStatus.Received,
            NewStatus = OrderStatus.InProgress,
            StatusNote = "Picked up",
        });
        context.OrderStatusHistories.Add(history);
        await context.SaveChangesAsync();

        var statusEvent = new OutgoingStatusEvent(new OutgoingStatusEventDetails
        {
            ExternalSystemId = externalSystem.Id,
            StatusHistoryId = history.Id,
        });
        context.OutgoingStatusEvents.Add(statusEvent);
        await context.SaveChangesAsync();

        return (statusEvent.Id, statusEvent.ServiceBusMessageId, order.ClientPortalOrderId, history.ChangedAt);
    }

    private sealed class RecordingStatusEventPublisher : IStatusEventPublisher
    {
        public List<StatusChangeNotification> Published { get; } = new();

        public Task PublishAsync(StatusChangeNotification notification, CancellationToken cancellationToken = default)
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }
    }
}

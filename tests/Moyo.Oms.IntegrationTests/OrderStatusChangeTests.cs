using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Application.Orders;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;
using Moyo.Oms.Infrastructure.Persistence;
using Moyo.Oms.Infrastructure.Persistence.Repositories;

using Xunit;

namespace Moyo.Oms.IntegrationTests;

[Collection("Database")]
public sealed class OrderStatusChangeTests
{
    private readonly SqlServerFixture _fixture;

    public OrderStatusChangeTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ChangeStatus_ValidTransition_WritesHistoryAndOutboxAtomically()
    {
        var (orderId, vendorUserId, externalSystemId) = await SeedReceivedOrderAsync();

        await using (var context = _fixture.CreateContext())
        {
            await BuildService(context, vendorUserId).ChangeStatusAsync(new ChangeOrderStatusRequest
            {
                OrderId = orderId,
                TargetStatus = OrderStatus.InProgress,
                StatusNote = "Picked up",
            });
        }

        await using (var context = _fixture.CreateContext())
        {
            var order = await context.CustomerOrders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.InProgress);

            var history = await context.OrderStatusHistories.SingleAsync(h => h.OrderId == orderId);
            history.PreviousStatus.Should().Be(OrderStatus.Received);
            history.NewStatus.Should().Be(OrderStatus.InProgress);
            history.ChangedByVendorUserId.Should().Be(vendorUserId);
            history.StatusNote.Should().Be("Picked up");

            var outbox = await context.OutgoingStatusEvents.SingleAsync(e => e.StatusHistoryId == history.Id);
            outbox.Status.Should().Be(PublishStatus.Pending);
            outbox.ExternalSystemId.Should().Be(externalSystemId);
        }
    }

    [Fact]
    public async Task ChangeStatus_IllegalTransition_ThrowsConflictAndWritesNothing()
    {
        var (orderId, vendorUserId, _) = await SeedReceivedOrderAsync();

        await using (var context = _fixture.CreateContext())
        {
            var act = () => BuildService(context, vendorUserId).ChangeStatusAsync(new ChangeOrderStatusRequest
            {
                OrderId = orderId,
                TargetStatus = OrderStatus.Completed,
            });

            await act.Should().ThrowAsync<ConflictException>();
        }

        await using (var context = _fixture.CreateContext())
        {
            var order = await context.CustomerOrders.FirstAsync(o => o.Id == orderId);
            order.Status.Should().Be(OrderStatus.Received);

            var historyCount = await context.OrderStatusHistories.CountAsync(h => h.OrderId == orderId);
            historyCount.Should().Be(0);

            var historyIds = await context.OrderStatusHistories
                .Where(h => h.OrderId == orderId)
                .Select(h => h.Id)
                .ToListAsync();
            var outboxLeaked = await context.OutgoingStatusEvents
                .AnyAsync(e => historyIds.Contains(e.StatusHistoryId));
            outboxLeaked.Should().BeFalse();
        }
    }

    private OrderStatusService BuildService(OmsDbContext context, int vendorUserId) =>
        new(
            new ChangeOrderStatusRequestValidator(),
            new OrderRepository(context),
            new OrderStatusHistoryRepository(context),
            new OutgoingStatusEventRepository(context),
            new StubCurrentVendorUser(vendorUserId),
            context);

    private async Task<(int OrderId, int VendorUserId, int ExternalSystemId)> SeedReceivedOrderAsync()
    {
        await using var context = _fixture.CreateContext();

        var externalSystem = new ExternalSystem("Client Portal", IntegrationType.ServiceBus);
        context.ExternalSystems.Add(externalSystem);
        var vendor = new Vendor("Acme Supplies");
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();

        var vendorUser = new VendorUser(new VendorUserRegistration
        {
            VendorId = vendor.Id,
            AzureAdUserId = Guid.NewGuid().ToString(),
            FullName = "Ops Person",
            EmailAddress = "ops@example.com",
            Role = VendorRole.VendorOperator,
        });
        context.VendorUsers.Add(vendorUser);
        await context.SaveChangesAsync();

        var incomingEvent = new IncomingOrderEvent(new IncomingOrderEventDetails
        {
            ExternalSystemId = externalSystem.Id,
            ServiceBusMessageId = Guid.NewGuid().ToString(),
            ClientPortalOrderId = "CPO-1",
        });
        var order = new CustomerOrder(incomingEvent, new CustomerOrderDetails
        {
            ClientPortalOrderId = "CPO-1",
            ClientReference = "REF-1",
        });
        context.CustomerOrders.Add(order);
        await context.SaveChangesAsync();

        return (order.Id, vendorUser.Id, externalSystem.Id);
    }

    private sealed class StubCurrentVendorUser : ICurrentVendorUserProvider
    {
        private readonly int _vendorUserId;

        public StubCurrentVendorUser(int vendorUserId) => _vendorUserId = vendorUserId;

        public Task<int> GetVendorUserIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_vendorUserId);
    }
}

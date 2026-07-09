using Microsoft.EntityFrameworkCore;

using Moyo.Oms.Application.Abstractions.Queries;
using Moyo.Oms.Application.Common;
using Moyo.Oms.Application.Orders;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Infrastructure.Persistence.Queries;

/// <summary>
/// EF Core read-side queries over customer orders, scoped to a vendor via allocation.
/// </summary>

public sealed class OrderQueries : IOrderQueries
{
    private readonly OmsDbContext _context;

    public OrderQueries(OmsDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrderListItem>> GetPageByVendorAsync(
        int vendorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<CustomerOrder> query = _context.CustomerOrders
            .AsNoTracking()
            .Where(order => _context.OrderAllocations
                .Any(allocation => allocation.OrderId == order.Id && allocation.VendorId == vendorId));

        int totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new
            {
                order.Id,
                order.ClientPortalOrderId,
                order.ClientReference,
                order.Status,
                order.FulfilmentStatus,
                order.OrderTotal,
                order.ReceivedAt,
                order.AllocatedAt,
            })
            .ToListAsync(cancellationToken);

        List<OrderListItem> items = rows
            .Select(row => new OrderListItem
            {
                Id = row.Id,
                ClientPortalOrderId = row.ClientPortalOrderId,
                ClientReference = row.ClientReference,
                Status = row.Status.ToString(),
                FulfilmentStatus = row.FulfilmentStatus.ToString(),
                OrderTotal = row.OrderTotal,
                ReceivedAt = row.ReceivedAt,
                AllocatedAt = row.AllocatedAt,
            })
            .ToList();

        return new PagedResult<OrderListItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<OrderDetail?> GetDetailForVendorAsync(
        int orderId,
        int vendorId,
        CancellationToken cancellationToken = default)
    {
        CustomerOrder? order = await _context.CustomerOrders
            .AsNoTracking()
            .Include(customerOrder => customerOrder.LineItems)
                .ThenInclude(lineItem => lineItem.ProductReference)
            .FirstOrDefaultAsync(
                customerOrder => customerOrder.Id == orderId
                    && _context.OrderAllocations.Any(allocation =>
                        allocation.OrderId == customerOrder.Id && allocation.VendorId == vendorId),
                cancellationToken);

        if (order is null)
        {
            return null;
        }

        List<OrderStatusHistory> history = await _context.OrderStatusHistories
            .AsNoTracking()
            .Where(entry => entry.OrderId == orderId)
            .OrderBy(entry => entry.ChangedAt)
            .ToListAsync(cancellationToken);

        return new OrderDetail
        {
            Id = order.Id,
            ClientPortalOrderId = order.ClientPortalOrderId,
            ClientReference = order.ClientReference,
            Status = order.Status.ToString(),
            FulfilmentStatus = order.FulfilmentStatus.ToString(),
            OrderTotal = order.OrderTotal,
            ReceivedAt = order.ReceivedAt,
            AllocatedAt = order.AllocatedAt,
            Lines = order.LineItems
                .Select(lineItem => new OrderDetailLine
                {
                    ProductReferenceId = lineItem.ProductReferenceId,
                    ProductName = lineItem.ProductReference.ProductName,
                    Quantity = lineItem.Quantity,
                    UnitPrice = lineItem.UnitPrice,
                    LineTotal = lineItem.LineTotal,
                })
                .ToList(),
            History = history
                .Select(entry => new OrderStatusHistoryItem
                {
                    PreviousStatus = entry.PreviousStatus.ToString(),
                    NewStatus = entry.NewStatus.ToString(),
                    StatusNote = entry.StatusNote,
                    ChangedAt = entry.ChangedAt,
                })
                .ToList(),
        };
    }
}

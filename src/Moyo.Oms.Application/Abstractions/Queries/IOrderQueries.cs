using Moyo.Oms.Application.Common;
using Moyo.Oms.Application.Orders;

namespace Moyo.Oms.Application.Abstractions.Queries;

/// <summary>
/// Read-side queries over customer orders, scoped to a vendor.
/// </summary>

public interface IOrderQueries
{
    Task<PagedResult<OrderListItem>> GetPageByVendorAsync(
        int vendorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<OrderDetail?> GetDetailForVendorAsync(
        int orderId,
        int vendorId,
        CancellationToken cancellationToken = default);
}

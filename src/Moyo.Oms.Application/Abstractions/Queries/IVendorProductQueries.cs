using Moyo.Oms.Application.Common;
using Moyo.Oms.Application.VendorProducts;

namespace Moyo.Oms.Application.Abstractions.Queries;

/// <summary>
/// Read-side queries over vendor products.
/// </summary>

public interface IVendorProductQueries
{
    Task<PagedResult<VendorProductListItem>> GetPageByVendorAsync(
        int vendorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

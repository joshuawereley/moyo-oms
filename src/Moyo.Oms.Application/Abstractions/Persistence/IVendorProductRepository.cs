using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Loads and stores vendor price/stock aggregates.
/// </summary>

public interface IVendorProductRepository
{
    Task<VendorProduct?> GetByIdAsync(int vendorProductId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VendorProduct>> GetByVendorAsync(int vendorId, CancellationToken cancellationToken = default);

    void Add(VendorProduct vendorProduct);
}

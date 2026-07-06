using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Stores immutable vendor-product change-history records.
/// </summary>

public interface IVendorProductChangeHistoryRepository
{
    void Add(VendorProductChangeHistory change);
}

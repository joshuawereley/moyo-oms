using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Abstractions.Persistence;

/// <summary>
/// Loads product references cached from the PMS.
/// </summary>

public interface IProductReferenceRepository
{
    Task<ProductReference?> GetByPmsProductIdAsync(
            int externalSystemId,
            string pmsProductId,
            CancellationToken cancellationToken = default);
}

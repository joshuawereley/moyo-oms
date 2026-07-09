namespace Moyo.Oms.Application.Abstractions.Products;

/// <summary>
/// Synchronously queries the Product Management System for product data.
/// </summary>

public interface IProductCatalogClient
{
    Task<PmsProduct?> GetProductAsync(string pmsProductId, CancellationToken cancellationToken = default);
}

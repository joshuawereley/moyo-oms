namespace Moyo.Oms.Application.Products;

/// <summary>
/// Synchronises products from the Product Management System into the local cache.
/// </summary>

public interface IProductSyncService
{
    Task SyncProductAsync(SyncProductRequest request, CancellationToken cancellationToken = default);
}

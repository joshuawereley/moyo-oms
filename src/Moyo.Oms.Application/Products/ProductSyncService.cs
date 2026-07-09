using FluentValidation;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Abstractions.Products;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.Products;

/// <summary>
/// Fetches a product from the PMS and caches it locally as a product reference.
/// </summary>

public sealed class ProductSyncService : IProductSyncService
{
    private readonly IValidator<SyncProductRequest> _validator;
    private readonly IProductCatalogClient _catalog;
    private readonly IProductReferenceRepository _productReferences;
    private readonly IUnitOfWork _unitOfWork;

    public ProductSyncService(
        IValidator<SyncProductRequest> validator,
        IProductCatalogClient catalog,
        IProductReferenceRepository productReferences,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _catalog = catalog;
        _productReferences = productReferences;
        _unitOfWork = unitOfWork;
    }

    public async Task SyncProductAsync(SyncProductRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        ProductReference? existing = await _productReferences.GetByPmsProductIdAsync(
            request.ExternalSystemId, request.PmsProductId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        PmsProduct product =
            await _catalog.GetProductAsync(request.PmsProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(PmsProduct), request.PmsProductId);

        var reference = new ProductReference(new ProductReferenceDetails
        {
            ExternalSystemId = request.ExternalSystemId,
            PmsProductId = product.PmsProductId,
            ProductName = product.ProductName,
            ProductCategory = product.ProductCategory,
        });

        if (!product.IsActive)
        {
            reference.Deactivate();
        }

        _productReferences.Add(reference);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

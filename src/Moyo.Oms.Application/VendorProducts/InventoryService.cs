using FluentValidation;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Entities;

namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// Handles vendor price and stock use cases.
/// </summary>

public sealed class InventoryService : IInventoryService
{
    private readonly IValidator<RepriceVendorProductRequest> _repriceValidator;
    private readonly IVendorProductRepository _vendorProducts;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        IValidator<RepriceVendorProductRequest> repriceValidator,
        IVendorProductRepository vendorProducts,
        IUnitOfWork unitOfWork)
    {
        _repriceValidator = repriceValidator;
        _vendorProducts = vendorProducts;
        _unitOfWork = unitOfWork;
    }

    public async Task RepriceAsync(
        RepriceVendorProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await _repriceValidator.ValidateAndThrowAsync(request, cancellationToken);

        VendorProduct vendorProduct =
            await _vendorProducts.GetByIdAsync(request.VendorProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(VendorProduct), request.VendorProductId);

        vendorProduct.Reprice(request.NewPrice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

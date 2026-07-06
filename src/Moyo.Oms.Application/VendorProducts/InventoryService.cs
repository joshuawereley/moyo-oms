using System.Globalization;

using FluentValidation;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// Handles vendor price and stock use cases.
/// </summary>

public sealed class InventoryService : IInventoryService
{
    private readonly IValidator<RepriceVendorProductRequest> _repriceValidator;
    private readonly IVendorProductRepository _vendorProducts;
    private readonly IVendorProductChangeHistoryRepository _changeHistory;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        IValidator<RepriceVendorProductRequest> repriceValidator,
        IVendorProductRepository vendorProducts,
        IVendorProductChangeHistoryRepository changeHistory,
        IUnitOfWork unitOfWork)
    {
        _repriceValidator = repriceValidator;
        _vendorProducts = vendorProducts;
        _changeHistory = changeHistory;
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

        decimal previousPrice = vendorProduct.SellingPrice;

        vendorProduct.Reprice(request.NewPrice);

        _changeHistory.Add(new VendorProductChangeHistory(new VendorProductChange
        {
            VendorProductId = vendorProduct.Id,
            ChangedByVendorUserId = request.ChangedByVendorUserId,
            ChangeType = ChangeType.Price,
            PreviousValue = previousPrice.ToString(CultureInfo.InvariantCulture),
            NewValue = vendorProduct.SellingPrice.ToString(CultureInfo.InvariantCulture),
        }));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

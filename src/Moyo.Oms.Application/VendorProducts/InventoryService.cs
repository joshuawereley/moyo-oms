using System.Globalization;

using FluentValidation;

using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

using static System.FormattableString;

namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// Handles vendor price and stock use cases.
/// </summary>

public sealed class InventoryService : IInventoryService
{
    private readonly IValidator<RepriceVendorProductRequest> _repriceValidator;
    private readonly IValidator<AdjustVendorProductStockRequest> _adjustStockValidator;
    private readonly IVendorProductRepository _vendorProducts;
    private readonly IVendorProductChangeHistoryRepository _changeHistory;
    private readonly ICurrentVendorUserProvider _currentVendorUser;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        IValidator<RepriceVendorProductRequest> repriceValidator,
        IValidator<AdjustVendorProductStockRequest> adjustStockValidator,
        IVendorProductRepository vendorProducts,
        IVendorProductChangeHistoryRepository changeHistory,
        ICurrentVendorUserProvider currentVendorUser,
        IUnitOfWork unitOfWork)
    {
        _repriceValidator = repriceValidator;
        _adjustStockValidator = adjustStockValidator;
        _vendorProducts = vendorProducts;
        _changeHistory = changeHistory;
        _currentVendorUser = currentVendorUser;
        _unitOfWork = unitOfWork;
    }

    public async Task RepriceAsync(
        RepriceVendorProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await _repriceValidator.ValidateAndThrowAsync(request, cancellationToken);

        int changedByVendorUserId =
            await _currentVendorUser.GetVendorUserIdAsync(cancellationToken);

        VendorProduct vendorProduct =
            await _vendorProducts.GetByIdAsync(request.VendorProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(VendorProduct), request.VendorProductId);

        decimal previousPrice = vendorProduct.SellingPrice;

        vendorProduct.Reprice(request.NewPrice);

        _changeHistory.Add(new VendorProductChangeHistory(new VendorProductChange
        {
            VendorProductId = vendorProduct.Id,
            ChangedByVendorUserId = changedByVendorUserId,
            ChangeType = ChangeType.Price,
            PreviousValue = previousPrice.ToString(CultureInfo.InvariantCulture),
            NewValue = vendorProduct.SellingPrice.ToString(CultureInfo.InvariantCulture),
        }));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AdjustStockAsync(
        AdjustVendorProductStockRequest request,
        CancellationToken cancellationToken = default)
    {
        await _adjustStockValidator.ValidateAndThrowAsync(request, cancellationToken);

        int changedByVendorUserId =
            await _currentVendorUser.GetVendorUserIdAsync(cancellationToken);

        VendorProduct vendorProduct =
            await _vendorProducts.GetByIdAsync(request.VendorProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(VendorProduct), request.VendorProductId);

        int previousStock = vendorProduct.StockQuantity;

        if (request.Direction == StockAdjustmentDirection.Increase)
        {
            vendorProduct.IncreaseStock(request.Quantity);
        }
        else
        {
            if (request.Quantity > vendorProduct.StockQuantity)
            {
                throw new ConflictException(Invariant(
                    $"Cannot decrease stock by {request.Quantity}; only {vendorProduct.StockQuantity} in stock for vendor product {vendorProduct.Id}."));
            }

            vendorProduct.DecreaseStock(request.Quantity);
        }

        _changeHistory.Add(new VendorProductChangeHistory(new VendorProductChange
        {
            VendorProductId = vendorProduct.Id,
            ChangedByVendorUserId = changedByVendorUserId,
            ChangeType = ChangeType.Stock,
            PreviousValue = previousStock.ToString(CultureInfo.InvariantCulture),
            NewValue = vendorProduct.StockQuantity.ToString(CultureInfo.InvariantCulture),
        }));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

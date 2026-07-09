using FluentValidation;

namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// Validates a request to adjust a vendor product's stock.
/// </summary>

public sealed class AdjustVendorProductStockRequestValidator
    : AbstractValidator<AdjustVendorProductStockRequest>
{
    public AdjustVendorProductStockRequestValidator()
    {
        RuleFor(request => request.VendorProductId).GreaterThan(0);
        RuleFor(request => request.Direction).IsInEnum();
        RuleFor(request => request.Quantity).GreaterThan(0);
    }
}

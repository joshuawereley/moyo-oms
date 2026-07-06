using FluentValidation;

namespace Moyo.Oms.Application.VendorProducts;

/// <summary>
/// Validates a request to reprice a vendor product.
/// </summary>

public sealed class RepriceVendorProductRequestValidator
    : AbstractValidator<RepriceVendorProductRequest>
{
    public RepriceVendorProductRequestValidator()
    {
        RuleFor(request => request.VendorProductId).GreaterThan(0);
        RuleFor(request => request.NewPrice).GreaterThanOrEqualTo(0m);
    }
}

using FluentValidation;

namespace Moyo.Oms.Application.Products;

/// <summary>
/// Validates a request to synchronise a product from the PMS.
/// </summary>

public sealed class SyncProductRequestValidator : AbstractValidator<SyncProductRequest>
{
    public SyncProductRequestValidator()
    {
        RuleFor(request => request.ExternalSystemId).GreaterThan(0);
        RuleFor(request => request.PmsProductId).NotEmpty();
    }
}

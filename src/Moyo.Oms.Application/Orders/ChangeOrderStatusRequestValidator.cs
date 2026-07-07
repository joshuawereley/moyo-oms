using FluentValidation;

using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Application.Orders;

/// <summary>
/// Validates a request to change a customer order's status.
/// </summary>

public sealed class ChangeOrderStatusRequestValidator
    : AbstractValidator<ChangeOrderStatusRequest>
{
    public ChangeOrderStatusRequestValidator()
    {
        RuleFor(request => request.OrderId).GreaterThan(0);

        RuleFor(request => request.TargetStatus)
            .Must(BeVendorSettable)
            .WithMessage("Target status must be InProgress, Completed, or Cancelled.");

        RuleFor(request => request.StatusNote)
            .MaximumLength(500);
    }

    private static bool BeVendorSettable(OrderStatus status) =>
        status is OrderStatus.InProgress or OrderStatus.Completed or OrderStatus.Cancelled;
}

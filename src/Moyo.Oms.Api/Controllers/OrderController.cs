using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Moyo.Oms.Api.Authorization;
using Moyo.Oms.Api.Contracts;
using Moyo.Oms.Application.Orders;

namespace Moyo.Oms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderStatusService _orderStatusService;

    public OrdersController(IOrderStatusService orderStatusService)
    {
        _orderStatusService = orderStatusService;
    }

    [HttpPut("{orderId:int}/status")]
    [Authorize(Policy = AuthorizationPolicies.VendorAdministrator)]
    public async Task<IActionResult> ChangeStatus(
        int orderId,
        ChangeOrderStatusBody body,
        CancellationToken cancellationToken)
    {
        ChangeOrderStatusRequest request = new()
        {
            OrderId = orderId,
            TargetStatus = body.TargetStatus,
            StatusNote = body.StatusNote,
        };

        await _orderStatusService.ChangeStatusAsync(request, cancellationToken);

        return NoContent();
    }
}

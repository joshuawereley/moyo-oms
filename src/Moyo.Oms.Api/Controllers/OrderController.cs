using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Moyo.Oms.Api.Authorization;
using Moyo.Oms.Api.Contracts;
using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Abstractions.Queries;
using Moyo.Oms.Application.Common;
using Moyo.Oms.Application.Orders;

namespace Moyo.Oms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderStatusService _orderStatusService;
    private readonly IOrderQueries _orderQueries;
    private readonly ICurrentVendorUserProvider _currentVendorUser;

    public OrdersController(
        IOrderStatusService orderStatusService,
        IOrderQueries orderQueries,
        ICurrentVendorUserProvider currentVendorUser)
    {
        _orderStatusService = orderStatusService;
        _orderQueries = orderQueries;
        _currentVendorUser = currentVendorUser;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderListItem>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        int vendorId = await _currentVendorUser.GetVendorIdAsync(cancellationToken);

        PagedResult<OrderListItem> result =
            await _orderQueries.GetPageByVendorAsync(vendorId, page, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<OrderDetail>> GetOrder(
        int orderId,
        CancellationToken cancellationToken)
    {
        int vendorId = await _currentVendorUser.GetVendorIdAsync(cancellationToken);

        OrderDetail? order = await _orderQueries.GetDetailForVendorAsync(orderId, vendorId, cancellationToken);

        return order is null ? NotFound() : Ok(order);
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

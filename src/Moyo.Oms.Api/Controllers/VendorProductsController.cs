using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Moyo.Oms.Api.Authorization;
using Moyo.Oms.Api.Contracts;
using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Abstractions.Queries;
using Moyo.Oms.Application.Common;
using Moyo.Oms.Application.VendorProducts;

namespace Moyo.Oms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/vendor-products")]
public sealed class VendorProductsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IVendorProductQueries _vendorProductQueries;
    private readonly ICurrentVendorUserProvider _currentVendorUser;

    public VendorProductsController(
        IInventoryService inventoryService,
        IVendorProductQueries vendorProductQueries,
        ICurrentVendorUserProvider currentVendorUser)
    {
        _inventoryService = inventoryService;
        _vendorProductQueries = vendorProductQueries;
        _currentVendorUser = currentVendorUser;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VendorProductListItem>>> GetMyProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        int vendorId = await _currentVendorUser.GetVendorIdAsync(cancellationToken);

        PagedResult<VendorProductListItem> result =
            await _vendorProductQueries.GetPageByVendorAsync(vendorId, page, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{vendorProductId:int}/price")]
    [Authorize(Policy = AuthorizationPolicies.VendorAdministrator)]
    public async Task<IActionResult> Reprice(
        int vendorProductId,
        RepriceVendorProductBody body,
        CancellationToken cancellationToken)
    {
        RepriceVendorProductRequest request = new()
        {
            VendorProductId = vendorProductId,
            NewPrice = body.NewPrice,
        };

        await _inventoryService.RepriceAsync(request, cancellationToken);

        return NoContent();
    }

    [HttpPut("{vendorProductId:int}/stock")]
    [Authorize(Policy = AuthorizationPolicies.VendorAdministrator)]
    public async Task<IActionResult> AdjustStock(
        int vendorProductId,
        AdjustVendorProductStockBody body,
        CancellationToken cancellationToken)
    {
        AdjustVendorProductStockRequest request = new()
        {
            VendorProductId = vendorProductId,
            Direction = body.Direction,
            Quantity = body.Quantity,
        };

        await _inventoryService.AdjustStockAsync(request, cancellationToken);

        return NoContent();
    }
}

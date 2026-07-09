using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Moyo.Oms.Api.Authorization;
using Moyo.Oms.Api.Contracts;
using Moyo.Oms.Application.VendorProducts;

namespace Moyo.Oms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/vendor-products")]
public sealed class VendorProductsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public VendorProductsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
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

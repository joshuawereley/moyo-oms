using Microsoft.AspNetCore.Mvc;

using Moyo.Oms.Api.Contracts;
using Moyo.Oms.Application.VendorProducts;

namespace Moyo.Oms.Api.Controllers;

[ApiController]
[Route("api/vendor-products")]
public sealed class VendorProductsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public VendorProductsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPut("{vendorProductId:int}/price")]
    public async Task<IActionResult> Reprice(
        int vendorProductId,
        RepriceVendorProductBody body,
        CancellationToken cancellationToken)
    {
        RepriceVendorProductRequest request = new()
        {
            VendorProductId = vendorProductId,
            NewPrice = body.NewPrice,
            ChangedByVendorUserId = body.ChangedByVendorUserId,
        };

        await _inventoryService.RepriceAsync(request, cancellationToken);

        return NoContent();
    }
}

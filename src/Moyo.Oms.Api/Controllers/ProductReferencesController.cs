using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Moyo.Oms.Api.Authorization;
using Moyo.Oms.Api.Contracts;
using Moyo.Oms.Application.Products;

namespace Moyo.Oms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/product-references")]
public sealed class ProductReferencesController : ControllerBase
{
    private readonly IProductSyncService _productSyncService;

    public ProductReferencesController(IProductSyncService productSyncService)
    {
        _productSyncService = productSyncService;
    }

    [HttpPost("sync")]
    [Authorize(Policy = AuthorizationPolicies.VendorAdministrator)]
    public async Task<IActionResult> Sync(
        SyncProductBody body,
        CancellationToken cancellationToken)
    {
        SyncProductRequest request = new()
        {
            ExternalSystemId = body.ExternalSystemId,
            PmsProductId = body.PmsProductId,
        };

        await _productSyncService.SyncProductAsync(request, cancellationToken);

        return NoContent();
    }
}

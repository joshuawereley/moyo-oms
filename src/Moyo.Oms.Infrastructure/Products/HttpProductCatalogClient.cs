using System.Net;
using System.Net.Http.Json;

using Moyo.Oms.Application.Abstractions.Products;

namespace Moyo.Oms.Infrastructure.Products;

/// <summary>
/// Queries the Product Management System over HTTP.
/// </summary>

public sealed class HttpProductCatalogClient : IProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public HttpProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PmsProduct?> GetProductAsync(string pmsProductId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pmsProductId);

        using HttpResponseMessage response = await _httpClient.GetAsync(
            new Uri($"products/{Uri.EscapeDataString(pmsProductId)}", UriKind.Relative),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PmsProduct>(cancellationToken);
    }
}

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Moyo.Oms.Application.VendorProducts;

namespace Moyo.Oms.Application;

/// <summary>
/// Registers Application-layer services with the DI container.
/// </summary>

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IInventoryService, InventoryService>();

        return services;
    }
}

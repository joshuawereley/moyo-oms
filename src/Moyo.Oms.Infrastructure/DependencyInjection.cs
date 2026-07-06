using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Infrastructure.Persistence;
using Moyo.Oms.Infrastructure.Persistence.Repositories;

namespace Moyo.Oms.Infrastructure;

/// <summary>
/// Registers Infrastructure-layer services with the DI container.
/// </summary>

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<OmsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<OmsDbContext>());

        services.AddScoped<IVendorProductRepository, VendorProductRepository>();
        services.AddScoped<IVendorProductChangeHistoryRepository, VendorProductChangeHistoryRepository>();

        services.AddScoped<IVendorUserRepository, VendorUserRepository>();
        return services;
    }
}

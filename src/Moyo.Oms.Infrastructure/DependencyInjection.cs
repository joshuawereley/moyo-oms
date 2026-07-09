using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moyo.Oms.Application.Abstractions.Persistence;
using Moyo.Oms.Application.Abstractions.Queries;
using Moyo.Oms.Infrastructure.Persistence;
using Moyo.Oms.Infrastructure.Persistence.Queries;
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

        services.AddScoped<IIncomingOrderEventRepository, IncomingOrderEventRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
        services.AddScoped<IOutgoingStatusEventRepository, OutgoingStatusEventRepository>();
        services.AddScoped<IOrderAllocationRepository, OrderAllocationRepository>();
        services.AddScoped<IProductReferenceRepository, ProductReferenceRepository>();

        services.AddScoped<IVendorProductQueries, VendorProductQueries>();
        services.AddScoped<IOrderQueries, OrderQueries>();

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moyo.Oms.AllocationFunction;
using Moyo.Oms.Application;
using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        string connectionString =
            context.Configuration.GetConnectionString("OmsDatabase")
            ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

        services.AddApplication();
        services.AddInfrastructure(connectionString);
        services.AddSingleton<ICurrentUser, NoCurrentUser>();
    })
    .Build();

host.Run();

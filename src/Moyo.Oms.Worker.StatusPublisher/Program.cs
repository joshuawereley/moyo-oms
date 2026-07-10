using Microsoft.Extensions.Options;

using Moyo.Oms.Application;
using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Abstractions.Messaging;
using Moyo.Oms.Application.Orders;
using Moyo.Oms.Infrastructure;
using Moyo.Oms.Worker.StatusPublisher;

var builder = Host.CreateApplicationBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("OmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddSingleton<ICurrentUser, NoCurrentUser>();

builder.Services.AddOptions<ServiceBusOptions>()
    .Bind(builder.Configuration.GetSection(ServiceBusOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ConnectionString)
            || !string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace),
        "ServiceBus requires either a ConnectionString or a FullyQualifiedNamespace.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.TopicName), "ServiceBus topic name is required.")
    .Validate(options => options.BatchSize > 0, "ServiceBus BatchSize must be positive.")
    .Validate(options => options.PollIntervalSeconds > 0, "ServiceBus PollIntervalSeconds must be positive.")
    .ValidateOnStart();

builder.Services.AddSingleton(serviceProvider =>
    ServiceBusClientFactory.Create(serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>().Value));

builder.Services.AddScoped<IOutboxPublisher, OutboxPublisher>();
builder.Services.AddSingleton<IStatusEventPublisher, ServiceBusStatusEventPublisher>();
builder.Services.AddHostedService<StatusPublisherWorker>();

var host = builder.Build();
host.Run();

using Moyo.Oms.Application;
using Moyo.Oms.Infrastructure;
using Moyo.Oms.Worker.Intake;

var builder = Host.CreateApplicationBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("OmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddOptions<ServiceBusOptions>()
    .Bind(builder.Configuration.GetSection(ServiceBusOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "ServiceBus connection string is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.TopicName), "ServiceBus topic name is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.SubscriptionName), "ServiceBus subscription name is required.")
    .Validate(options => options.ExternalSystemId > 0, "ServiceBus ExternalSystemId must be positive.")
    .ValidateOnStart();

var host = builder.Build();
host.Run();

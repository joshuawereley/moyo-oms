using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Options;

using Moyo.Oms.Application.Common.Exceptions;
using Moyo.Oms.Application.Orders;
using Moyo.Oms.Contracts;

namespace Moyo.Oms.Worker.Intake;

/// <summary>
/// Subscribes to the orders.new topic and turns messages into customer orders.
/// </summary>

public sealed class OrderIntakeProcessor : IHostedService, IAsyncDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusOptions _options;
    private readonly ILogger<OrderIntakeProcessor> _logger;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;

    public OrderIntakeProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<ServiceBusOptions> options,
        ILogger<OrderIntakeProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;

        _client = new ServiceBusClient(_options.ConnectionString);
        _processor = _client.CreateProcessor(
            _options.TopicName,
            _options.SubscriptionName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1,
            });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;
        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            OrderPlaced? message = JsonSerializer.Deserialize<OrderPlaced>(args.Message.Body.ToString());

            if (message is null)
            {
                throw new InvalidOperationException("Message body was empty.");
            }

            if (message.Lines.Count == 0)
            {
                throw new InvalidOperationException("Order has no line items.");
            }

            CreateOrderRequest request = new()
            {
                ExternalSystemId = _options.ExternalSystemId,
                ServiceBusMessageId = args.Message.MessageId,
                ClientPortalOrderId = message.ClientPortalOrderId,
                ClientReference = message.ClientReference,
                Lines = message.Lines
                    .Select(line => new CreateOrderLine
                    {
                        PmsProductId = line.PmsProductId,
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice,
                    })
                    .ToList(),
            };

            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            await orderService.CreateOrderAsync(request, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or NotFoundException)
        {
            _logger.LogError(exception, "Dead-lettering unprocessable message {MessageId}.", args.Message.MessageId);
            await args.DeadLetterMessageAsync(
                args.Message,
                "UnprocessableMessage",
                exception.Message,
                args.CancellationToken);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error from {ErrorSource}.", args.ErrorSource);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }
}

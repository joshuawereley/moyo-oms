using Microsoft.Extensions.Options;

using Moyo.Oms.Application.Orders;

namespace Moyo.Oms.Worker.StatusPublisher;

/// <summary>
/// Periodically drains pending outbox rows to Service Bus.
/// </summary>

public sealed class StatusPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusOptions _options;
    private readonly ILogger<StatusPublisherWorker> _logger;

    public StatusPublisherWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<ServiceBusOptions> options,
        ILogger<StatusPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

        do
        {
            try
            {
                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IOutboxPublisher publisher = scope.ServiceProvider.GetRequiredService<IOutboxPublisher>();

                int published = await publisher.PublishPendingAsync(_options.BatchSize, stoppingToken);

                if (published > 0)
                {
                    _logger.LogInformation("Published {Count} outbox message(s).", published);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Failed to drain the status outbox; will retry next cycle.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

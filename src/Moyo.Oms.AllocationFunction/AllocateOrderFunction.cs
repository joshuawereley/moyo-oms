using System.Text.Json;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Moyo.Oms.Application.Orders;
using Moyo.Oms.Contracts;

namespace Moyo.Oms.AllocationFunction;

/// <summary>
/// Allocates an order to a vendor when an OrderReceived event arrives.
/// </summary>

public sealed class AllocateOrderFunction
{
    private readonly IAllocationService _allocationService;
    private readonly ILogger<AllocateOrderFunction> _logger;

    public AllocateOrderFunction(IAllocationService allocationService, ILogger<AllocateOrderFunction> logger)
    {
        _allocationService = allocationService;
        _logger = logger;
    }

    [Function(nameof(AllocateOrderFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("%OrderReceivedTopic%", "%AllocationSubscription%", Connection = "ServiceBusConnection")]
        string messageBody,
        CancellationToken cancellationToken)
    {
        OrderReceived message =
            JsonSerializer.Deserialize<OrderReceived>(messageBody)
            ?? throw new InvalidOperationException("Message body was empty.");

        AllocationOutcome outcome = await _allocationService.AllocateAsync(message.OrderId, cancellationToken);

        _logger.LogInformation("Allocation for order {OrderId}: {Outcome}.", message.OrderId, outcome);
    }
}

namespace Moyo.Oms.Worker.StatusPublisher;

/// <summary>
/// Configuration for the order-status publisher's Service Bus topic and polling.
/// </summary>

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 50;
    public int PollIntervalSeconds { get; set; } = 10;
}

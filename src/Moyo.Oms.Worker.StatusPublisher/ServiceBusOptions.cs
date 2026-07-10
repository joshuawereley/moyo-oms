namespace Moyo.Oms.Worker.StatusPublisher;

/// <summary>
/// Configuration for the order-status publisher's Service Bus topic and polling.
/// </summary>

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    /// <summary>
    /// Used when <see cref="FullyQualifiedNamespace"/> is empty, which locally means the emulator.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Set in Azure to authenticate with managed identity instead of a shared access key.
    /// </summary>
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    public string TopicName { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 50;
    public int PollIntervalSeconds { get; set; } = 10;
}

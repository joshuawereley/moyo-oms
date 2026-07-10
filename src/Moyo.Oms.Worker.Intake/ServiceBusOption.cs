namespace Moyo.Oms.Worker.Intake;

/// <summary>
/// Configuration for the Order Intake Service Bus subscription.
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
    public string SubscriptionName { get; set; } = string.Empty;
    public string OrderReceivedTopicName { get; set; } = string.Empty;
    public int ExternalSystemId { get; set; }
}

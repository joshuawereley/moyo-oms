namespace Moyo.Oms.Worker.Intake;

/// <summary>
/// Configuration for the Order Intake Service Bus subscription.
/// </summary>

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public int ExternalSystemId { get; set; }
}

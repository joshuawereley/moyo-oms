using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace Moyo.Oms.Worker.Intake;

/// <summary>
/// Builds the Service Bus client, preferring managed identity over a shared access key.
/// </summary>

internal static class ServiceBusClientFactory
{
    public static ServiceBusClient Create(ServiceBusOptions options)
    {
        // DefaultAzureCredential reads AZURE_CLIENT_ID to pick the user-assigned identity.
        // The emulator has no identity endpoint, so local runs fall back to the connection string.
        return string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace)
            ? new ServiceBusClient(options.ConnectionString)
            : new ServiceBusClient(options.FullyQualifiedNamespace, new DefaultAzureCredential());
    }
}

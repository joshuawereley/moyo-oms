using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data required to register a new vendor user.
/// </summary>

public sealed record VendorUserRegistration
{
    public required int VendorId { get; init; }
    public required string AzureAdUserId { get; init; }
    public required string FullName { get; init; }
    public required string EmailAddress { get; init; }
    public required VendorRole Role { get; init; }
}

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// The data captured when a vendor user's session begins.
/// </summary>

public sealed record UserSessionStart
{
    public required int VendorUserId { get; init; }
    public required string TokenId { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}

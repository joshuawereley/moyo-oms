using Moyo.Oms.Application.Abstractions.Identity;

namespace Moyo.Oms.AllocationFunction;

/// <summary>
/// ICurrentUser for the function host, which has no authenticated user.
/// </summary>

public sealed class NoCurrentUser : ICurrentUser
{
    public string? ExternalUserId => null;
}

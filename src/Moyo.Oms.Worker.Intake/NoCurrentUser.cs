using Moyo.Oms.Application.Abstractions.Identity;

namespace Moyo.Oms.Worker.Intake;

/// <summary>
/// ICurrentUser for the worker host, which has no authenticated user.
/// </summary>

public sealed class NoCurrentUser : ICurrentUser
{
    public string? ExternalUserId => null;
}

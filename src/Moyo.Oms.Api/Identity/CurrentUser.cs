using Microsoft.Identity.Web;

using Moyo.Oms.Application.Abstractions.Identity;

namespace Moyo.Oms.Api.Identity;

/// <summary>
/// Resolves the authenticated caller's external id from the HTTP context.
/// </summary>

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? ExternalUserId =>
        _httpContextAccessor.HttpContext?.User.GetObjectId();
}

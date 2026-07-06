namespace Moyo.Oms.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated caller is not permitted to perform an action.
/// </summary>

public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message) { }
}

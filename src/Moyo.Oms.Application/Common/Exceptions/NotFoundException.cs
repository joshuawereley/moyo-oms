namespace Moyo.Oms.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// </summary>

public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"\"{name}\" with id {key} was not found.")
    {
    }
}

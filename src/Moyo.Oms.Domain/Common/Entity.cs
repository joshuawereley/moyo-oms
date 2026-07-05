namespace Moyo.Oms.Domain.Common;

/// <summary>
/// Base type for domain entities identified by a database-generated key.
/// <summary>

public abstract class Entity
{
    public int Id { get; protected set; }
}

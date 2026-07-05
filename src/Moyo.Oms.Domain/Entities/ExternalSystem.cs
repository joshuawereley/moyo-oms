using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

// <summary>
// An external system the OMS integrates with (e.g. Client Portal or Product
// Management System).
// </summary>

public class ExternalSystem : Entity
{
    private ExternalSystem() { }

    public ExternalSystem(string systemName, IntegrationType integrationType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemName);

        SystemName = systemName;
        IntegrationType = integrationType;
        IsActive = true;
    }

    public string SystemName { get; private set; } = null!;

    public IntegrationType IntegrationType { get; private set; }

    public bool IsActive { get; private set; }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}

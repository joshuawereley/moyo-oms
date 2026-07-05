using Moyo.Oms.Domain.Common;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A local reference to an approved product owned by the PMS.
/// </summary>

public class ProductReference : Entity
{
    private ProductReference()
    {
    }

    public ProductReference(ProductReferenceDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(details.ExternalSystemId);
        ArgumentException.ThrowIfNullOrWhiteSpace(details.PmsProductId);
        ArgumentException.ThrowIfNullOrWhiteSpace(details.ProductName);
        ArgumentException.ThrowIfNullOrWhiteSpace(details.ProductCategory);

        ExternalSystemId = details.ExternalSystemId;
        PmsProductId = details.PmsProductId;
        ProductName = details.ProductName;
        ProductCategory = details.ProductCategory;
        IsActive = true;
        LastCheckedAt = DateTimeOffset.UtcNow;
    }

    public int ExternalSystemId { get; private set; }

    public ExternalSystem ExternalSystem { get; private set; } = null!;

    public string PmsProductId { get; private set; } = null!;

    public string ProductName { get; private set; } = null!;

    public string ProductCategory { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTimeOffset LastCheckedAt { get; private set; }

    public void MarkChecked() => LastCheckedAt = DateTimeOffset.UtcNow;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}

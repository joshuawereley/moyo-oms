using Moyo.Oms.Domain.Common;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Entities;

/// <summary>
/// A vendor's price and stock for a specific product.
/// </summary>

public class VendorProduct : Entity
{
    private VendorProduct()
    {
    }

    public VendorProduct(VendorProductListing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(listing.VendorId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(listing.ProductReferenceId);
        ArgumentOutOfRangeException.ThrowIfNegative(listing.SellingPrice);
        ArgumentOutOfRangeException.ThrowIfNegative(listing.StockQuantity);

        VendorId = listing.VendorId;
        ProductReferenceId = listing.ProductReferenceId;
        SellingPrice = listing.SellingPrice;
        StockQuantity = listing.StockQuantity;
        Availability = StockQuantity > 0
            ? AvailabilityStatus.InStock
            : AvailabilityStatus.OutOfStock;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public int VendorId { get; private set; }

    public Vendor Vendor { get; private set; } = null!;

    public int ProductReferenceId { get; private set; }

    public ProductReference ProductReference { get; private set; } = null!;

    public decimal SellingPrice { get; private set; }

    public int StockQuantity { get; private set; }

    public AvailabilityStatus Availability { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Reprice(decimal newPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(newPrice);

        SellingPrice = newPrice;
        MarkUpdated();
    }

    public void IncreaseStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        StockQuantity += quantity;
        SyncAvailability();
        MarkUpdated();
    }

    public void DecreaseStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException("Cannot decrease stock below zero.");
        }

        StockQuantity -= quantity;
        SyncAvailability();
        MarkUpdated();
    }

    public void Discontinue()
    {
        Availability = AvailabilityStatus.Discontinued;
        MarkUpdated();
    }

    private void SyncAvailability()
    {
        if (Availability == AvailabilityStatus.Discontinued)
        {
            return;
        }

        Availability = StockQuantity > 0
            ? AvailabilityStatus.InStock
            : AvailabilityStatus.OutOfStock;
    }

    private void MarkUpdated() => UpdatedAt = DateTimeOffset.UtcNow;
}

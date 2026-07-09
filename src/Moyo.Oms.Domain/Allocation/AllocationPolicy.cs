using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

namespace Moyo.Oms.Domain.Allocation;

/// <summary>
/// Selects the vendor to fulfil an order, based on price and inventory:
/// the cheapest vendor that has every line in stock in sufficient quantity.
/// </summary>

public static class AllocationPolicy
{
    public static AllocationSelection? SelectVendor(
        IReadOnlyCollection<LineRequirement> lines,
        IReadOnlyCollection<VendorProduct> candidates)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(candidates);

        if (lines.Count == 0)
        {
            return null;
        }

        AllocationSelection? best = null;

        foreach (int vendorId in candidates.Select(candidate => candidate.VendorId).Distinct())
        {
            decimal totalPrice = 0m;
            bool coversAllLines = true;

            foreach (LineRequirement line in lines)
            {
                VendorProduct? listing = candidates.FirstOrDefault(candidate =>
                    candidate.VendorId == vendorId &&
                    candidate.ProductReferenceId == line.ProductReferenceId &&
                    candidate.Availability == AvailabilityStatus.InStock &&
                    candidate.StockQuantity >= line.Quantity);

                if (listing is null)
                {
                    coversAllLines = false;
                    break;
                }

                totalPrice += listing.SellingPrice * line.Quantity;
            }

            if (!coversAllLines)
            {
                continue;
            }

            if (best is null || totalPrice < best.TotalPrice ||
                (totalPrice == best.TotalPrice && vendorId < best.VendorId))
            {
                best = new AllocationSelection { VendorId = vendorId, TotalPrice = totalPrice };
            }
        }

        return best;
    }
}

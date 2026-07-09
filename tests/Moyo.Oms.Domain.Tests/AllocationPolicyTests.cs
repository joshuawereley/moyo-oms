using FluentAssertions;

using Moyo.Oms.Domain.Allocation;
using Moyo.Oms.Domain.Entities;

using Xunit;

namespace Moyo.Oms.Domain.Tests;

public class AllocationPolicyTests
{
    [Fact]
    public void SelectVendor_WithOneInStockVendor_SelectsItWithTotalPrice()
    {
        var lines = new[] { Line(productReferenceId: 1, quantity: 2) };
        var candidates = new[] { Listing(vendorId: 5, productReferenceId: 1, price: 10m, stock: 3) };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection.Should().NotBeNull();
        selection!.VendorId.Should().Be(5);
        selection.TotalPrice.Should().Be(20m);
    }

    [Fact]
    public void SelectVendor_WithTwoInStockVendors_SelectsCheaper()
    {
        var lines = new[] { Line(productReferenceId: 1, quantity: 1) };
        var candidates = new[]
        {
            Listing(vendorId: 1, productReferenceId: 1, price: 15m, stock: 5),
            Listing(vendorId: 2, productReferenceId: 1, price: 12m, stock: 5),
        };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection!.VendorId.Should().Be(2);
    }

    [Fact]
    public void SelectVendor_WhenCheapestIsOutOfStock_SelectsInStockVendor()
    {
        var lines = new[] { Line(productReferenceId: 1, quantity: 1) };
        var candidates = new[]
        {
            Listing(vendorId: 1, productReferenceId: 1, price: 8m, stock: 0),
            Listing(vendorId: 2, productReferenceId: 1, price: 12m, stock: 5),
        };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection!.VendorId.Should().Be(2);
    }

    [Fact]
    public void SelectVendor_WhenCheapestHasInsufficientStock_SelectsVendorWithEnough()
    {
        var lines = new[] { Line(productReferenceId: 1, quantity: 4) };
        var candidates = new[]
        {
            Listing(vendorId: 1, productReferenceId: 1, price: 8m, stock: 3),
            Listing(vendorId: 2, productReferenceId: 1, price: 12m, stock: 10),
        };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection!.VendorId.Should().Be(2);
    }

    [Fact]
    public void SelectVendor_WhenDiscontinued_IsNotEligible()
    {
        var lines = new[] { Line(productReferenceId: 1, quantity: 1) };
        var discontinued = Listing(vendorId: 1, productReferenceId: 1, price: 8m, stock: 5);
        discontinued.Discontinue();
        var candidates = new[]
        {
            discontinued,
            Listing(vendorId: 2, productReferenceId: 1, price: 12m, stock: 5),
        };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection!.VendorId.Should().Be(2);
    }

    [Fact]
    public void SelectVendor_WithMultipleLines_RequiresVendorToCoverAll()
    {
        var lines = new[]
        {
            Line(productReferenceId: 1, quantity: 1),
            Line(productReferenceId: 2, quantity: 1),
        };
        var candidates = new[]
        {
            Listing(vendorId: 1, productReferenceId: 1, price: 5m, stock: 5),
            Listing(vendorId: 2, productReferenceId: 1, price: 6m, stock: 5),
            Listing(vendorId: 2, productReferenceId: 2, price: 7m, stock: 5),
        };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection!.VendorId.Should().Be(2);
        selection.TotalPrice.Should().Be(13m);
    }

    [Fact]
    public void SelectVendor_WithNoEligibleVendor_ReturnsNull()
    {
        var lines = new[] { Line(productReferenceId: 1, quantity: 1) };
        var candidates = new[] { Listing(vendorId: 1, productReferenceId: 1, price: 8m, stock: 0) };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection.Should().BeNull();
    }

    [Fact]
    public void SelectVendor_WithNoLines_ReturnsNull()
    {
        var selection = AllocationPolicy.SelectVendor(
            Array.Empty<LineRequirement>(),
            new[] { Listing(vendorId: 1, productReferenceId: 1, price: 8m, stock: 5) });

        selection.Should().BeNull();
    }

    [Fact]
    public void SelectVendor_OnPriceTie_PrefersLowerVendorId()
    {
        var lines = new[] { Line(productReferenceId: 1, quantity: 1) };
        var candidates = new[]
        {
            Listing(vendorId: 7, productReferenceId: 1, price: 10m, stock: 5),
            Listing(vendorId: 3, productReferenceId: 1, price: 10m, stock: 5),
        };

        var selection = AllocationPolicy.SelectVendor(lines, candidates);

        selection!.VendorId.Should().Be(3);
    }

    private static LineRequirement Line(int productReferenceId, int quantity) =>
        new() { ProductReferenceId = productReferenceId, Quantity = quantity };

    private static VendorProduct Listing(int vendorId, int productReferenceId, decimal price, int stock) =>
        new(new VendorProductListing
        {
            VendorId = vendorId,
            ProductReferenceId = productReferenceId,
            SellingPrice = price,
            StockQuantity = stock,
        });
}

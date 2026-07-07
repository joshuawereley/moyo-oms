using FluentAssertions;

using Moyo.Oms.Domain.Entities;
using Moyo.Oms.Domain.Enums;

using Xunit;

namespace Moyo.Oms.Domain.Tests;

public class VendorProductTests
{
    [Fact]
    public void Constructor_WithValidListing_CopiesListingOntoProduct()
    {
        var listing = Listing(vendorId: 7, productReferenceId: 42, sellingPrice: 199.99m, stockQuantity: 3);

        var product = new VendorProduct(listing);

        product.VendorId.Should().Be(7);
        product.ProductReferenceId.Should().Be(42);
        product.SellingPrice.Should().Be(199.99m);
        product.StockQuantity.Should().Be(3);
    }

    [Fact]
    public void Constructor_WithStockOnHand_MarksInStock()
    {
        var product = new VendorProduct(Listing(stockQuantity: 1));

        product.Availability.Should().Be(AvailabilityStatus.InStock);
    }

    [Fact]
    public void Constructor_WithZeroStock_MarksOutOfStock()
    {
        var product = new VendorProduct(Listing(stockQuantity: 0));

        product.Availability.Should().Be(AvailabilityStatus.OutOfStock);
    }

    [Fact]
    public void Constructor_WithNullListing_Throws()
    {
        var act = () => new VendorProduct(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveVendorId_Throws(int vendorId)
    {
        var act = () => new VendorProduct(Listing(vendorId: vendorId));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveProductReferenceId_Throws(int productReferenceId)
    {
        var act = () => new VendorProduct(Listing(productReferenceId: productReferenceId));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithNegativePrice_Throws()
    {
        var act = () => new VendorProduct(Listing(sellingPrice: -0.01m));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithNegativeStock_Throws()
    {
        var act = () => new VendorProduct(Listing(stockQuantity: -1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static VendorProductListing Listing(
        int vendorId = 1,
        int productReferenceId = 1,
        decimal sellingPrice = 100m,
        int stockQuantity = 5) =>
        new()
        {
            VendorId = vendorId,
            ProductReferenceId = productReferenceId,
            SellingPrice = sellingPrice,
            StockQuantity = stockQuantity,
        };
}

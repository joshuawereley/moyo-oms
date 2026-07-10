namespace Moyo.Oms.Seeder;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    /// <summary>
    /// Drops and recreates the database first. Safe locally, destructive against a deployed
    /// database: it issues DROP DATABASE, taking the SKU and any external users with it.
    /// </summary>
    public bool Reset { get; set; } = true;

    public int Vendors { get; set; } = 4;
    public int Products { get; set; } = 200;
    public int Orders { get; set; } = 5000;

    public string? PrimaryVendorAzureAdUserId { get; set; }
    public string? SecondaryVendorAzureAdUserId { get; set; }
}

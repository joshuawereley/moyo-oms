namespace Moyo.Oms.Seeder;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public int Vendors { get; set; } = 4;
    public int Products { get; set; } = 200;
    public int Orders { get; set; } = 5000;

    public string? PrimaryVendorAzureAdUserId { get; set; }
    public string? SecondaryVendorAzureAdUserId { get; set; }
}

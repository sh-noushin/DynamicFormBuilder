namespace FormBuilder.Core.Options;

public sealed class SeedOptions
{
    public const string SectionName = "SeedData";

    public string AdminPassword { get; set; } = string.Empty;
    public string UserPassword { get; set; } = string.Empty;
}

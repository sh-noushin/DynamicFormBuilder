namespace FormBuilder.Core.Options;

public sealed class SeedOptions
{
    public const string SectionName = "SeedData";

    public string AdminPassword { get; set; } = string.Empty;
    public string UserPassword { get; set; } = string.Empty;
    // Vendor-level super admin. Provisioned only at seed time; there is
    // no in-app path to create another one. Used to log in and manage
    // customer workspaces.
    public string SuperAdminPassword { get; set; } = string.Empty;
}

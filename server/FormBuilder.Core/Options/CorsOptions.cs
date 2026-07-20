namespace FormBuilder.Core.Options;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public const string AngularPolicyName = "AllowAngular";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

namespace FormBuilder.Models.Entities;

// A tenant / workspace. Every Form, User, and ApiKey belongs to exactly
// one Organization; queries in the authenticated API are always scoped
// by the current caller's OrganizationId so tenants never see each
// other's data. Public form endpoints (/f/:slug) are addressed by slug,
// which is globally unique across all orgs.
public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Short URL-safe identifier reserved for future white-label / custom
    // domain features. Not used for routing today.
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

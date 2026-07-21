using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Core.DTOs;

// Public shape of a tenant. Slug is exposed so super admin can share
// signup URLs like /register/{slug} with a new customer.
public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    // Convenience count fields for the super-admin tenants table.
    public int UserCount { get; set; }
    public int FormCount { get; set; }
}

// Super admin provisions a workspace + optionally the first tenant admin
// so the customer can log in immediately after we create their account.
public class CreateOrganizationDto
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    // Optional — if provided we also create the first tenant admin so
    // the customer can start using the app right away. Leave blank if
    // the tenant admin will be provisioned later via a separate call.
    [StringLength(64, MinimumLength = 3)]
    public string? AdminUsername { get; set; }

    [EmailAddress, StringLength(256)]
    public string? AdminEmail { get; set; }

    [StringLength(256, MinimumLength = 6)]
    public string? AdminPassword { get; set; }
}

public class UpdateOrganizationDto
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}

// Returned by the impersonate endpoint — a JWT scoped to the target
// tenant, plus the tenant's name so the frontend can show a "You are
// impersonating X" banner.
public class ImpersonationResultDto
{
    public string Token { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace FormBuilder.Models.Entities;

public class User : IdentityUser
{
    // Tenant the user belongs to. Set when the user registers (creates
    // the org) or when an admin creates them (inherits admin's org).
    public Guid OrganizationId { get; set; }
}
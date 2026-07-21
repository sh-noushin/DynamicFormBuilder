using System.Security.Claims;
using FormBuilder.Core.Interfaces;

namespace FormBuilder.API.Services;

// Pulls the current caller's OrganizationId from the authenticated
// ClaimsPrincipal. Registered as scoped so the value is stable within
// a request; IHttpContextAccessor is safe because Program.cs registers
// it via AddHttpContextAccessor().
public class CurrentUserService : ICurrentUserService
{
    // Custom claim name for the tenant id. Kept as a string constant so
    // JwtService and ApiKey auth handler can emit and read the same key.
    public const string OrgIdClaimType = "orgId";

    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid GetOrganizationId()
    {
        var id = GetOrganizationIdOrNull();
        if (id == null)
            throw new InvalidOperationException("No authenticated user or missing orgId claim.");
        return id.Value;
    }

    public Guid? GetOrganizationIdOrNull()
    {
        var user = _accessor.HttpContext?.User;
        var raw = user?.FindFirstValue(OrgIdClaimType);
        return Guid.TryParse(raw, out var id) ? id : (Guid?)null;
    }
}

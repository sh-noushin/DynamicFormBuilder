using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

// Super-admin surface for managing customer workspaces. Every method
// on this interface must be invoked from a controller guarded by
// [Authorize(Roles = Roles.SuperAdmin)] — the service itself does not
// re-check the role.
public interface IOrganizationService
{
    Task<IEnumerable<OrganizationDto>> ListAsync();
    Task<OrganizationDto> CreateAsync(CreateOrganizationDto payload);
    Task<OrganizationDto> UpdateAsync(Guid id, UpdateOrganizationDto payload);
    Task DeleteAsync(Guid id);
    // Mints a JWT scoped to the target tenant with the Admin role, so
    // super admin can walk through the app "as" that tenant's admin
    // for support / debugging.
    Task<ImpersonationResultDto> ImpersonateAsync(Guid id);
}

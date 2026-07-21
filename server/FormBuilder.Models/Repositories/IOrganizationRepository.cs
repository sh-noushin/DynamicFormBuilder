using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id);
    Task<Organization?> GetBySlugAsync(string slug);
    Task<Organization> CreateAsync(Organization organization);
    Task<Organization?> UpdateAsync(Guid id, string name);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Organization>> ListAsync();
    // Aggregate counts for the super-admin tenants table. Returned as
    // a Dictionary keyed by OrganizationId so a single call can populate
    // the table without an N+1 pattern.
    Task<Dictionary<Guid, int>> GetUserCountsAsync();
    Task<Dictionary<Guid, int>> GetFormCountsAsync();
}

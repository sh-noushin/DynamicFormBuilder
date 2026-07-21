using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id);
    Task<Organization> CreateAsync(Organization organization);
}

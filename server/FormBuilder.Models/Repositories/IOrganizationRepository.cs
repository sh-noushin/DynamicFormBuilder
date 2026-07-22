using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id);
    Task<Organization?> GetBySlugAsync(string slug);
    Task<Organization?> GetByStripeCustomerIdAsync(string stripeCustomerId);
    Task<Organization?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
    Task<Organization> CreateAsync(Organization organization);
    Task<Organization?> UpdateAsync(Guid id, string name);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Organization>> ListAsync();
    // Aggregate counts for the super-admin tenants table. Returned as
    // a Dictionary keyed by OrganizationId so a single call can populate
    // the table without an N+1 pattern.
    Task<Dictionary<Guid, int>> GetUserCountsAsync();
    Task<Dictionary<Guid, int>> GetFormCountsAsync();
    // Persists billing-related field changes for a given tenant.
    // Called from BillingService after Stripe webhook events or the
    // checkout completion flow.
    Task<Organization?> UpdateBillingAsync(Guid id, System.Action<Organization> mutate);
    Task<int> GetFormCountAsync(Guid organizationId);
    Task<int> GetSubmissionsThisMonthAsync(Guid organizationId);
}

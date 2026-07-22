using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly FormBuilderDbContext _context;

    public OrganizationRepository(FormBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid id)
    {
        return await _context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Organization?> GetBySlugAsync(string slug)
    {
        return await _context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Slug == slug);
    }

    public async Task<Organization?> GetByStripeCustomerIdAsync(string stripeCustomerId)
    {
        if (string.IsNullOrEmpty(stripeCustomerId)) return null;
        return await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.StripeCustomerId == stripeCustomerId);
    }

    public async Task<Organization?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        if (string.IsNullOrEmpty(stripeSubscriptionId)) return null;
        return await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.StripeSubscriptionId == stripeSubscriptionId);
    }

    public async Task<Organization> CreateAsync(Organization organization)
    {
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();
        return organization;
    }

    public async Task<Organization?> UpdateAsync(Guid id, string name)
    {
        var existing = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (existing == null) return null;
        existing.Name = name;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Organization?> UpdateBillingAsync(Guid id, Action<Organization> mutate)
    {
        var existing = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (existing == null) return null;
        mutate(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (existing == null) return false;
        _context.Organizations.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Organization>> ListAsync()
    {
        return await _context.Organizations
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, int>> GetUserCountsAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .GroupBy(u => u.OrganizationId)
            .Select(g => new { OrgId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrgId, x => x.Count);
    }

    public async Task<Dictionary<Guid, int>> GetFormCountsAsync()
    {
        return await _context.Forms
            .IgnoreQueryFilters()
            .AsNoTracking()
            .GroupBy(f => f.OrganizationId)
            .Select(g => new { OrgId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrgId, x => x.Count);
    }

    public async Task<int> GetFormCountAsync(Guid organizationId)
    {
        // Count all forms for a specific tenant. Uses IgnoreQueryFilters
        // so the plan-limit check works even from an anonymous context
        // (e.g. impersonation, webhook processing).
        return await _context.Forms
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(f => f.OrganizationId == organizationId);
    }

    public async Task<int> GetSubmissionsThisMonthAsync(Guid organizationId)
    {
        // Count submissions received in the current UTC calendar month
        // across all forms in the tenant. Joins through FormVersion -> Form
        // to reach the organization scope.
        var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return await _context.FormSubmissions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(s => s.FormVersion.Form.OrganizationId == organizationId && s.SubmittedAt >= firstOfMonth);
    }
}

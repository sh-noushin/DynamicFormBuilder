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
        // Bypass the tenant filter — super admin needs to count across
        // every org, and Users aren't filtered anyway (Identity uses
        // explicit scoping in UserService), so this is just aggregate.
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
}

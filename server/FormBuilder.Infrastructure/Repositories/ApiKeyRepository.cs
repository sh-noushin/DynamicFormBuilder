using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly FormBuilderDbContext _context;

    public ApiKeyRepository(FormBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ApiKey>> GetAllAsync()
    {
        return await _context.ApiKeys
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<ApiKey?> GetByHashAsync(string keyHash)
    {
        // Called from the API-key auth handler before any user identity
        // has been established — bypass the tenant filter so we can find
        // the key by its globally-unique hash. The caller is then trusted
        // to be acting inside the key's OrganizationId.
        return await _context.ApiKeys
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash);
    }

    public async Task<ApiKey> CreateAsync(ApiKey apiKey)
    {
        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();
        return apiKey;
    }

    public async Task<ApiKey?> RevokeAsync(Guid id)
    {
        var key = await _context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id);
        if (key == null) return null;
        if (key.RevokedAt == null) key.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return key;
    }

    public async Task TouchLastUsedAsync(Guid id, DateTime timestamp)
    {
        try
        {
            var key = await _context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id);
            if (key == null) return;
            key.LastUsedAt = timestamp;
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Best-effort - never fail auth because a stat write failed.
        }
    }
}

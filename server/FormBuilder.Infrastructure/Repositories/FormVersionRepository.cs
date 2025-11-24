using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Repositories;

public class FormVersionRepository : IFormVersionRepository
{
    private readonly FormBuilderDbContext _context;

    public FormVersionRepository(FormBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FormVersion>> GetVersionsByFormIdAsync(Guid formId)
    {
        return await _context.FormVersions
            .Where(v => v.FormId == formId)
            .Include(v => v.Fields.OrderBy(f => f.Order))
            .OrderBy(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<FormVersion?> GetVersionAsync(Guid formId, int versionNumber)
    {
        return await _context.FormVersions
            .Include(v => v.Fields.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(v => v.FormId == formId && v.VersionNumber == versionNumber);
    }

    public async Task<FormVersion?> GetVersionByIdAsync(Guid versionId)
    {
        return await _context.FormVersions
            .Include(v => v.Fields.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(v => v.Id == versionId);
    }

    public async Task<FormVersion?> GetCurrentVersionAsync(Guid formId)
    {
        return await _context.FormVersions
            .Include(v => v.Fields.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(v => v.FormId == formId && v.IsCurrentVersion);
    }

    public async Task<FormVersion> CreateAsync(FormVersion version)
    {
        _context.FormVersions.Add(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task<FormVersion?> UpdateAsync(Guid formId, int versionNumber, FormVersion version)
    {
        var existingVersion = await _context.FormVersions
            .Include(v => v.Fields)
            .FirstOrDefaultAsync(v => v.FormId == formId && v.VersionNumber == versionNumber);

        if (existingVersion == null)
            return null;

        existingVersion.Description = version.Description;
        existingVersion.IsPublished = version.IsPublished;
        existingVersion.IsCurrentVersion = version.IsCurrentVersion;
        existingVersion.UpdatedAt = version.UpdatedAt;

        if (version.Fields.Any())
        {
            _context.FormVersionFields.RemoveRange(existingVersion.Fields);
            existingVersion.Fields = version.Fields;
        }

        await _context.SaveChangesAsync();
        return existingVersion;
    }

    public async Task<bool> DeleteAsync(Guid formId, int versionNumber)
    {
        var version = await _context.FormVersions
            .FirstOrDefaultAsync(v => v.FormId == formId && v.VersionNumber == versionNumber);

        if (version == null)
            return false;

        _context.FormVersions.Remove(version);
        await _context.SaveChangesAsync();
        return true;
    }
}

using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Repositories;

public class FormSubmissionRepository : IFormSubmissionRepository
{
    private readonly FormBuilderDbContext _context;

    public FormSubmissionRepository(FormBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<FormSubmission> CreateAsync(FormSubmission submission)
    {
        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task<FormSubmission?> GetByIdAsync(Guid id)
    {
        return await _context.FormSubmissions
            .Include(s => s.Values)
            .Include(s => s.FormVersion)
                .ThenInclude(v => v.Fields)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<FormSubmission>> GetByFormVersionIdAsync(Guid formVersionId)
    {
        return await _context.FormSubmissions
            .Include(s => s.Values)
            .Where(s => s.FormVersionId == formVersionId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FormSubmission>> GetByFormIdAsync(Guid formId)
    {
        return await _context.FormSubmissions
            .Include(s => s.Values)
            .Include(s => s.FormVersion)
            .Where(s => s.FormVersion.FormId == formId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<int> GetSubmissionCountByFormVersionIdAsync(Guid formVersionId)
    {
        return await _context.FormSubmissions
            .CountAsync(s => s.FormVersionId == formVersionId);
    }

    public async Task<int> GetSubmissionCountByFormIdAsync(Guid formId)
    {
        return await _context.FormSubmissions
            .CountAsync(s => s.FormVersion.FormId == formId);
    }

    public async Task<bool> HasSubmissionFromEmailAsync(Guid formId, string email)
    {
        // Case-insensitive because every mailbox provider we care about
        // treats the local part as case-insensitive in practice.
        var normalized = email.Trim().ToLower();
        return await _context.FormSubmissions
            .AnyAsync(s => s.FormVersion.FormId == formId
                        && s.SubmitterEmail != null
                        && s.SubmitterEmail.ToLower() == normalized);
    }

    public async Task<IReadOnlyList<DateTime>> GetSubmittedAtByFormIdSinceAsync(Guid formId, DateTime since)
    {
        return await _context.FormSubmissions
            .AsNoTracking()
            .Where(s => s.FormVersion.FormId == formId && s.SubmittedAt >= since)
            .OrderBy(s => s.SubmittedAt)
            .Select(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<FormSubmission?> UpdateAsync(Guid id, FormSubmission source)
    {
        var submission = await _context.FormSubmissions
            .Include(s => s.Values)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (submission == null)
            return null;

        submission.SubmitterName = source.SubmitterName;
        submission.SubmitterEmail = source.SubmitterEmail;
        submission.AdminNotes = source.AdminNotes;
        submission.SubmittedAt = DateTime.UtcNow;

        _context.FormSubmissionValues.RemoveRange(submission.Values);
        submission.Values.Clear();

        foreach (var value in source.Values)
        {
            submission.Values.Add(new FormSubmissionValue
            {
                FieldName = value.FieldName,
                FieldValue = value.FieldValue
            });
        }

        await _context.SaveChangesAsync();
        await _context.Entry(submission).Collection(s => s.Values).LoadAsync();

        return submission;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var submission = await _context.FormSubmissions.FindAsync(id);
        if (submission == null)
            return false;

        _context.FormSubmissions.Remove(submission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FormSubmission?> UpdateAdminNotesAsync(Guid id, string? adminNotes)
    {
        var submission = await _context.FormSubmissions
            .Include(s => s.Values)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (submission == null) return null;

        submission.AdminNotes = adminNotes;
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task<int> DeleteManyByFormAsync(Guid formId, IReadOnlyList<Guid> ids)
    {
        if (ids.Count == 0) return 0;

        var toDelete = await _context.FormSubmissions
            .Where(s => s.FormVersion.FormId == formId && ids.Contains(s.Id))
            .ToListAsync();

        if (toDelete.Count == 0) return 0;

        _context.FormSubmissions.RemoveRange(toDelete);
        await _context.SaveChangesAsync();
        return toDelete.Count;
    }
}

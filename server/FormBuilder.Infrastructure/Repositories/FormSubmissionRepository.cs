using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
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

    public async Task<FormSubmission> UpdateAsync(Guid id, string? submitterName, string? submitterEmail, Dictionary<string, string?> fieldValues)
    {
        var submission = await _context.FormSubmissions
            .Include(s => s.Values)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (submission == null)
        {
            throw new FormSubmissionNotFoundException(id);
        }

        submission.SubmitterName = submitterName;
        submission.SubmitterEmail = submitterEmail;
        submission.SubmittedAt = DateTime.UtcNow;

        _context.FormSubmissionValues.RemoveRange(submission.Values);
        submission.Values.Clear();

        foreach (var kvp in fieldValues)
        {
            submission.Values.Add(new FormSubmissionValue
            {
                FieldName = kvp.Key,
                FieldValue = kvp.Value
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
}

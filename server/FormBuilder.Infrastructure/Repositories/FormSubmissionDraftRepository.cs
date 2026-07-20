using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Repositories;

public class FormSubmissionDraftRepository : IFormSubmissionDraftRepository
{
    private readonly FormBuilderDbContext _context;

    public FormSubmissionDraftRepository(FormBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<FormSubmissionDraft?> GetByTokenAsync(Guid resumeToken)
    {
        return await _context.FormSubmissionDrafts
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ResumeToken == resumeToken);
    }

    public async Task<FormSubmissionDraft> CreateAsync(FormSubmissionDraft draft)
    {
        _context.FormSubmissionDrafts.Add(draft);
        await _context.SaveChangesAsync();
        return draft;
    }

    public async Task<FormSubmissionDraft?> UpdateAsync(FormSubmissionDraft draft)
    {
        var existing = await _context.FormSubmissionDrafts
            .FirstOrDefaultAsync(d => d.ResumeToken == draft.ResumeToken);
        if (existing == null) return null;

        existing.SubmitterName = draft.SubmitterName;
        existing.SubmitterEmail = draft.SubmitterEmail;
        existing.FieldValuesJson = draft.FieldValuesJson;
        existing.UpdatedAt = draft.UpdatedAt;
        existing.ExpiresAt = draft.ExpiresAt;
        await _context.SaveChangesAsync();
        return existing;
    }
}

using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FormBuilder.Infrastructure.Repositories;

public class FormRepository : IFormRepository
{
    private readonly FormBuilderDbContext _context;

    public FormRepository(FormBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Form>> GetAllAsync()
    {
        return await _context.Forms
            .Include(f => f.Versions.OrderBy(v => v.VersionNumber))
                .ThenInclude(v => v.Fields.OrderBy(field => field.Order))
            .ToListAsync();
    }

    public async Task<Form?> GetByIdAsync(Guid id)
    {
        return await _context.Forms
            .Include(f => f.Versions.OrderBy(v => v.VersionNumber))
                .ThenInclude(v => v.Fields.OrderBy(field => field.Order))
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Form?> GetBySlugAsync(string slug)
    {
        return await _context.Forms
            .Include(f => f.Versions.OrderBy(v => v.VersionNumber))
                .ThenInclude(v => v.Fields.OrderBy(field => field.Order))
            .FirstOrDefaultAsync(f => f.Slug == slug);
    }

    public async Task<Form> CreateAsync(Form form)
    {
        _context.Forms.Add(form);
        await _context.SaveChangesAsync();
        return form;
    }

    public async Task<Form?> UpdateAsync(Guid id, Form form)
    {
        var existingForm = await _context.Forms.FirstOrDefaultAsync(f => f.Id == id);

        if (existingForm == null)
            return null;

        existingForm.Name = form.Name;
        existingForm.Description = form.Description;
        existingForm.BrandColor = form.BrandColor;
        existingForm.AccessPassword = form.AccessPassword;
        existingForm.ThankYouMessage = form.ThankYouMessage;
        existingForm.RedirectUrl = form.RedirectUrl;
        existingForm.MaxSubmissions = form.MaxSubmissions;
        existingForm.ClosesAt = form.ClosesAt;
        existingForm.WebhookUrl = form.WebhookUrl;
        existingForm.WebhookSecret = form.WebhookSecret;
        existingForm.OneResponsePerEmail = form.OneResponsePerEmail;
        existingForm.IsActive = form.IsActive;

        await _context.SaveChangesAsync();
        return existingForm;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var form = await _context.Forms.FindAsync(id);
        if (form == null)
            return false;

        _context.Forms.Remove(form);
        await _context.SaveChangesAsync();
        return true;
    }
}

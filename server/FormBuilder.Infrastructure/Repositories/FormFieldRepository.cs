using FormBuilder.Infrastructure.Data;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Repositories;

public class FormFieldRepository : IFormFieldRepository
{
    private readonly FormBuilderDbContext _context;

    public FormFieldRepository(FormBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FormVersionField>> GetFieldsByVersionIdAsync(Guid versionId)
    {
        return await _context.FormVersionFields
            .Where(f => f.FormVersionId == versionId)
            .OrderBy(f => f.Order)
            .ToListAsync();
    }

    public async Task<FormVersionField?> GetByIdAsync(Guid fieldId)
    {
        return await _context.FormVersionFields
            .FirstOrDefaultAsync(f => f.Id == fieldId);
    }

    public async Task<FormVersionField> CreateAsync(FormVersionField field)
    {
        _context.FormVersionFields.Add(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task<FormVersionField?> UpdateAsync(Guid fieldId, FormVersionField field)
    {
        var existingField = await _context.FormVersionFields
            .FirstOrDefaultAsync(f => f.Id == fieldId);

        if (existingField == null)
            return null;

        ApplyChanges(existingField, field);
        await _context.SaveChangesAsync();
        return existingField;
    }

    public async Task<bool> DeleteAsync(Guid fieldId)
    {
        var field = await _context.FormVersionFields
            .FirstOrDefaultAsync(f => f.Id == fieldId);

        if (field == null)
            return false;

        _context.FormVersionFields.Remove(field);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<FormVersionField>> BulkCreateAsync(List<FormVersionField> fields)
    {
        _context.FormVersionFields.AddRange(fields);
        await _context.SaveChangesAsync();
        return fields;
    }

    public async Task<bool> BulkUpdateAsync(List<FormVersionField> fields)
    {
        foreach (var field in fields)
        {
            var existingField = await _context.FormVersionFields
                .FirstOrDefaultAsync(f => f.Id == field.Id);

            if (existingField != null)
            {
                ApplyChanges(existingField, field);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static void ApplyChanges(FormVersionField target, FormVersionField source)
    {
        target.Name = source.Name;
        target.Label = source.Label;
        target.Type = source.Type;
        target.IsRequired = source.IsRequired;
        target.Validation = source.Validation;
        target.DefaultValue = source.DefaultValue;
        target.Options = source.Options;
        target.Placeholder = source.Placeholder;
        target.HelpText = source.HelpText;
        target.ShowIfCondition = source.ShowIfCondition;
        target.Order = source.Order;
        target.IsVisible = source.IsVisible;
        target.IsReadOnly = source.IsReadOnly;
    }
}

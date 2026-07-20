using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IFormService
{
    Task<IEnumerable<FormDto>> GetAllFormsAsync();
    Task<FormDto> GetFormByIdAsync(Guid id);
    Task<FormDto> CreateFormAsync(CreateFormDto formDto);
    Task<FormDto> UpdateFormAsync(Guid id, UpdateFormDto formDto);
    Task<bool> DeleteFormAsync(Guid id);
    Task<bool> ActivateFormAsync(Guid id);
    Task<bool> DeactivateFormAsync(Guid id);
    Task<FormDto> DuplicateFormAsync(Guid id);
    // Returns a portable JSON-friendly snapshot of the form. Secrets
    // (AccessPassword, WebhookSecret) are deliberately excluded so the
    // export can be checked in / emailed without leaking credentials.
    Task<FormExportDto> ExportFormAsync(Guid id);
    // Creates a brand-new form from an exported snapshot. Slug is
    // regenerated; timestamps and IsActive are set fresh; a single
    // current+published version carries the exported fields.
    Task<FormDto> ImportFormAsync(FormExportDto payload);
}

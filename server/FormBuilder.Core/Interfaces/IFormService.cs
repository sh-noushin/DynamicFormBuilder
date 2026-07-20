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
}

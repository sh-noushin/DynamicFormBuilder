using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IFormFieldService
{
    Task<IEnumerable<FormFieldDto>> GetFieldsByVersionIdAsync(Guid versionId);
    Task<FormFieldDto> GetFieldByIdAsync(Guid fieldId);
    Task<FormFieldDto> CreateFieldAsync(CreateFormFieldDto fieldDto);
    Task<FormFieldDto> UpdateFieldAsync(Guid fieldId, UpdateFormFieldDto fieldDto);
    Task<bool> DeleteFieldAsync(Guid fieldId);
    Task<bool> ReorderFieldsAsync(Guid versionId, List<Guid> fieldIds);
    Task<IEnumerable<FormFieldDto>> BulkCreateFieldsAsync(Guid versionId, List<CreateFormFieldDto> fieldDtos);
    Task<bool> BulkUpdateFieldsAsync(Guid versionId, List<UpdateFormFieldDto> fieldDtos);
}

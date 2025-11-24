using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IFormVersionService
{
    Task<IEnumerable<FormVersionDto>> GetVersionsByFormIdAsync(Guid formId);
    Task<FormVersionDto> GetVersionAsync(Guid formId, int versionNumber);
    Task<FormVersionDto> GetVersionByIdAsync(Guid versionId);
    Task<FormVersionDto> GetCurrentVersionAsync(Guid formId);
    Task<FormVersionDto> CreateVersionAsync(Guid formId, CreateFormVersionDto versionDto);
    Task<FormVersionDto> UpdateVersionAsync(Guid formId, int versionNumber, UpdateFormVersionDto versionDto);
    Task<bool> DeleteVersionAsync(Guid formId, int versionNumber);
    Task<bool> PublishVersionAsync(Guid formId, int versionNumber);
    Task<bool> SetCurrentVersionAsync(Guid formId, int versionNumber);
}

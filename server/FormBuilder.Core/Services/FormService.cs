using AutoMapper;
using FormBuilder.Core.Common;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class FormService : IFormService
{
    private readonly IFormRepository _formRepository;
    private readonly IMapper _mapper;

    public FormService(IFormRepository formRepository, IMapper mapper)
    {
        _formRepository = formRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FormDto>> GetAllFormsAsync()
    {
        var forms = await _formRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<FormDto>>(forms);
    }

    public async Task<FormDto> GetFormByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var form = await _formRepository.GetByIdAsync(id);
        if (form == null)
            throw new FormNotFoundException(id);

        return _mapper.Map<FormDto>(form);
    }

    public async Task<FormDto> CreateFormAsync(CreateFormDto formDto)
    {
        if (formDto == null)
            throw new ArgumentNullException(nameof(formDto), "Form cannot be null.");

        var entity = _mapper.Map<FormBuilder.Models.Entities.Form>(formDto);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Slug = SlugGenerator.Generate();
        foreach (var version in entity.Versions)
        {
            version.CreatedAt = DateTime.UtcNow;
            version.UpdatedAt = DateTime.UtcNow;
        }
        var created = await _formRepository.CreateAsync(entity);
        return _mapper.Map<FormDto>(created);
    }

    public async Task<FormDto> UpdateFormAsync(Guid id, UpdateFormDto formDto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));
        if (formDto == null)
            throw new ArgumentNullException(nameof(formDto), "Form cannot be null.");

        var entity = _mapper.Map<FormBuilder.Models.Entities.Form>(formDto);
        entity.UpdatedAt = DateTime.UtcNow;
        var updatedForm = await _formRepository.UpdateAsync(id, entity);
        if (updatedForm == null)
            throw new FormNotFoundException(id);
        return _mapper.Map<FormDto>(updatedForm);
    }

    public async Task<bool> DeleteFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var result = await _formRepository.DeleteAsync(id);
        if (!result)
            throw new FormNotFoundException(id);
        return result;
    }

    public async Task<bool> ActivateFormAsync(Guid id) => await SetFormActiveStateAsync(id, true);

    public async Task<bool> DeactivateFormAsync(Guid id) => await SetFormActiveStateAsync(id, false);

    private async Task<bool> SetFormActiveStateAsync(Guid id, bool isActive)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var form = await _formRepository.GetByIdAsync(id);
        if (form == null)
            throw new FormNotFoundException(id);

        form.IsActive = isActive;
        form.UpdatedAt = DateTime.UtcNow;
        var result = await _formRepository.UpdateAsync(id, form);
        return result != null;
    }
}

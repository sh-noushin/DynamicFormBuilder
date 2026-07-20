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

    public async Task<FormDto> DuplicateFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        var source = await _formRepository.GetByIdAsync(id);
        if (source == null)
            throw new FormNotFoundException(id);

        var now = DateTime.UtcNow;
        var currentVersion = source.Versions.FirstOrDefault(v => v.IsCurrentVersion) ?? source.Versions.LastOrDefault();

        var clone = new FormBuilder.Models.Entities.Form
        {
            Name = $"Copy of {source.Name}",
            Description = source.Description,
            BrandColor = source.BrandColor,
            Slug = SlugGenerator.Generate(),
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now,
            Versions = new List<FormBuilder.Models.Entities.FormVersion>()
        };

        if (currentVersion != null)
        {
            var newVersion = new FormBuilder.Models.Entities.FormVersion
            {
                VersionNumber = 1,
                Description = currentVersion.Description,
                CreatedAt = now,
                UpdatedAt = now,
                IsPublished = false,
                IsCurrentVersion = true,
                Fields = currentVersion.Fields
                    .OrderBy(f => f.Order)
                    .Select(f => new FormBuilder.Models.Entities.FormVersionField
                    {
                        Name = f.Name,
                        Label = f.Label,
                        Type = f.Type,
                        IsRequired = f.IsRequired,
                        Validation = f.Validation,
                        DefaultValue = f.DefaultValue,
                        Options = f.Options,
                        Placeholder = f.Placeholder,
                        HelpText = f.HelpText,
                        ShowIfCondition = f.ShowIfCondition,
                        Order = f.Order,
                        IsVisible = f.IsVisible,
                        IsReadOnly = f.IsReadOnly
                    })
                    .ToList()
            };
            clone.Versions.Add(newVersion);
        }

        var created = await _formRepository.CreateAsync(clone);
        return _mapper.Map<FormDto>(created);
    }

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

using AutoMapper;
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
        try
        {
            var forms = await _formRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<FormDto>>(forms);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving forms.", ex);
        }
    }

    public async Task<FormDto> GetFormByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        try
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null)
                throw new FormNotFoundException(id);

            return _mapper.Map<FormDto>(form);
        }
        catch (FormNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving form with ID {id}.", ex);
        }
    }

    public async Task<FormDto> CreateFormAsync(CreateFormDto formDto)
    {
        if (formDto == null)
            throw new ArgumentNullException(nameof(formDto), "Form cannot be null.");

        try
        {
            var entity = _mapper.Map<FormBuilder.Models.Entities.Form>(formDto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            foreach (var version in entity.Versions)
            {
                version.CreatedAt = DateTime.UtcNow;
                version.UpdatedAt = DateTime.UtcNow;
            }
            var created = await _formRepository.CreateAsync(entity);
            return _mapper.Map<FormDto>(created);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the form.", ex);
        }
    }

    public async Task<FormDto> UpdateFormAsync(Guid id, UpdateFormDto formDto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));
        if (formDto == null)
            throw new ArgumentNullException(nameof(formDto), "Form cannot be null.");

        try
        {
            var entity = _mapper.Map<FormBuilder.Models.Entities.Form>(formDto);
            entity.UpdatedAt = DateTime.UtcNow;
            var updatedForm = await _formRepository.UpdateAsync(id, entity);
            if (updatedForm == null)
                throw new FormNotFoundException(id);
            return _mapper.Map<FormDto>(updatedForm);
        }
        catch (FormNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating form with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        try
        {
            var result = await _formRepository.DeleteAsync(id);
            if (!result)
                throw new FormNotFoundException(id);
            return result;
        }
        catch (FormNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting form with ID {id}.", ex);
        }
    }

    public async Task<bool> ActivateFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        try
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null)
                throw new FormNotFoundException(id);
            form.IsActive = true;
            form.UpdatedAt = DateTime.UtcNow;
            var result = await _formRepository.UpdateAsync(id, form);
            return result != null;
        }
        catch (FormNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while activating form with ID {id}.", ex);
        }
    }

    public async Task<bool> DeactivateFormAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(id));

        try
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null)
                throw new FormNotFoundException(id);
            form.IsActive = false;
            form.UpdatedAt = DateTime.UtcNow;
            var result = await _formRepository.UpdateAsync(id, form);
            return result != null;
        }
        catch (FormNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deactivating form with ID {id}.", ex);
        }
    }
}

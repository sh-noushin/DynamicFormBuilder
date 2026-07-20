using AutoMapper;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class FormFieldService : IFormFieldService
{
    private readonly IFormFieldRepository _fieldRepository;
    private readonly IMapper _mapper;

    public FormFieldService(IFormFieldRepository fieldRepository, IMapper mapper)
    {
        _fieldRepository = fieldRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FormFieldDto>> GetFieldsByVersionIdAsync(Guid versionId)
    {
        if (versionId == Guid.Empty)
            throw new ArgumentException("Version ID cannot be empty.", nameof(versionId));

        var fields = await _fieldRepository.GetFieldsByVersionIdAsync(versionId);
        return _mapper.Map<IEnumerable<FormFieldDto>>(fields);
    }

    public async Task<FormFieldDto> GetFieldByIdAsync(Guid fieldId)
    {
        if (fieldId == Guid.Empty)
            throw new ArgumentException("Field ID cannot be empty.", nameof(fieldId));

        var field = await _fieldRepository.GetByIdAsync(fieldId);
        if (field == null)
            throw new FormVersionFieldNotFoundException(fieldId);

        return _mapper.Map<FormFieldDto>(field);
    }

    public async Task<FormFieldDto> CreateFieldAsync(CreateFormFieldDto fieldDto)
    {
        if (fieldDto == null)
            throw new ArgumentNullException(nameof(fieldDto), "Field cannot be null.");

        var entity = _mapper.Map<FormBuilder.Models.Entities.FormVersionField>(fieldDto);
        var created = await _fieldRepository.CreateAsync(entity);
        return _mapper.Map<FormFieldDto>(created);
    }

    public async Task<FormFieldDto> UpdateFieldAsync(Guid fieldId, UpdateFormFieldDto fieldDto)
    {
        if (fieldId == Guid.Empty)
            throw new ArgumentException("Field ID cannot be empty.", nameof(fieldId));
        if (fieldDto == null)
            throw new ArgumentNullException(nameof(fieldDto), "Field cannot be null.");

        var entity = _mapper.Map<FormBuilder.Models.Entities.FormVersionField>(fieldDto);
        var updated = await _fieldRepository.UpdateAsync(fieldId, entity);
        if (updated == null)
            throw new FormVersionFieldNotFoundException(fieldId);
        return _mapper.Map<FormFieldDto>(updated);
    }

    public async Task<bool> DeleteFieldAsync(Guid fieldId)
    {
        if (fieldId == Guid.Empty)
            throw new ArgumentException("Field ID cannot be empty.", nameof(fieldId));

        var result = await _fieldRepository.DeleteAsync(fieldId);
        if (!result)
            throw new FormVersionFieldNotFoundException(fieldId);
        return result;
    }

    public async Task<bool> ReorderFieldsAsync(Guid versionId, List<Guid> fieldIds)
    {
        if (versionId == Guid.Empty)
            throw new ArgumentException("Version ID cannot be empty.", nameof(versionId));
        if (fieldIds == null)
            throw new ArgumentNullException(nameof(fieldIds), "Field IDs list cannot be null.");

        var fields = await _fieldRepository.GetFieldsByVersionIdAsync(versionId);
        var fieldsToUpdate = new List<FormBuilder.Models.Entities.FormVersionField>();

        for (int i = 0; i < fieldIds.Count; i++)
        {
            var field = fields.FirstOrDefault(f => f.Id == fieldIds[i]);
            if (field != null)
            {
                field.Order = i + 1;
                fieldsToUpdate.Add(field);
            }
        }

        return await _fieldRepository.BulkUpdateAsync(fieldsToUpdate);
    }

    public async Task<IEnumerable<FormFieldDto>> BulkCreateFieldsAsync(Guid versionId, List<CreateFormFieldDto> fieldDtos)
    {
        if (versionId == Guid.Empty)
            throw new ArgumentException("Version ID cannot be empty.", nameof(versionId));
        if (fieldDtos == null)
            throw new ArgumentNullException(nameof(fieldDtos), "Fields list cannot be null.");

        var entities = _mapper.Map<List<FormBuilder.Models.Entities.FormVersionField>>(fieldDtos);
        for (int i = 0; i < entities.Count; i++)
        {
            entities[i].FormVersionId = versionId;
            if (entities[i].Order == 0)
                entities[i].Order = i + 1;
        }
        var created = await _fieldRepository.BulkCreateAsync(entities);
        return _mapper.Map<IEnumerable<FormFieldDto>>(created);
    }

    public async Task<bool> BulkUpdateFieldsAsync(Guid versionId, List<UpdateFormFieldDto> fieldDtos)
    {
        if (versionId == Guid.Empty)
            throw new ArgumentException("Version ID cannot be empty.", nameof(versionId));
        if (fieldDtos == null)
            throw new ArgumentNullException(nameof(fieldDtos), "Fields list cannot be null.");

        var entities = _mapper.Map<List<FormBuilder.Models.Entities.FormVersionField>>(fieldDtos);
        foreach (var field in entities)
        {
            field.FormVersionId = versionId;
        }
        return await _fieldRepository.BulkUpdateAsync(entities);
    }
}

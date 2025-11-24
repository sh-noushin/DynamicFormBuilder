using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/forms/{formId}/versions/{versionNumber}/fields")]
[Authorize] 
public class FormFieldsController : ControllerBase
{
    private readonly IFormVersionService _formVersionService;
    private readonly IFormFieldService _formFieldService;
    private readonly AutoMapper.IMapper _mapper;

    public FormFieldsController(IFormVersionService formVersionService, IFormFieldService formFieldService, AutoMapper.IMapper mapper)
    {
        _formVersionService = formVersionService;
        _formFieldService = formFieldService;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")] 
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<FormFieldDto>), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<IEnumerable<FormFieldDto>>> GetFormFields(Guid formId, int versionNumber)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        if (version == null)
        {
            return NotFound();
        }

        var fields = await _formFieldService.GetFieldsByVersionIdAsync(version.Id);
        var fieldDtos = fields.Select(f => _mapper.Map<FormFieldDto>(f));
        return Ok(fieldDtos);
    }

    [HttpGet("{fieldId}")]
    [Authorize(Roles = "Admin,User")] 
    [ProducesResponseType(typeof(FormFieldDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormFieldDto>> GetFormField(Guid formId, int versionNumber, Guid fieldId)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        if (version == null)
        {
            return NotFound();
        }

        var field = await _formFieldService.GetFieldByIdAsync(fieldId);
        if (field == null || field.FormVersionId != version.Id)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<FormFieldDto>(field));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] 
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormFieldDto), 201)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormFieldDto>> CreateFormField(Guid formId, int versionNumber, CreateFormFieldDto createFieldDto)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        if (version == null)
        {
            return NotFound();
        }

        createFieldDto.FormVersionId = version.Id;
        var createdField = await _formFieldService.CreateFieldAsync(createFieldDto);
        return CreatedAtAction(nameof(GetFormField), new { formId = formId, versionNumber = versionNumber, fieldId = createdField.Id }, createdField);
    }

    [HttpPut("{fieldId}")]
    [Authorize(Roles = "Admin")] 
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormFieldDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormFieldDto>> UpdateFormField(Guid formId, int versionNumber, Guid fieldId, UpdateFormFieldDto updateFieldDto)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        if (version == null)
        {
            return NotFound();
        }

        var existingField = await _formFieldService.GetFieldByIdAsync(fieldId);
        if (existingField == null || existingField.FormVersionId != version.Id)
        {
            return NotFound();
        }

        updateFieldDto.FormVersionId = version.Id;
        var updatedField = await _formFieldService.UpdateFieldAsync(fieldId, updateFieldDto);
        if (updatedField == null)
        {
            return NotFound();
        }
        return Ok(updatedField);
    }

    [HttpDelete("{fieldId}")]
    [Authorize(Roles = "Admin")] 
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> DeleteFormField(Guid formId, int versionNumber, Guid fieldId)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        if (version == null)
        {
            return NotFound();
        }

        var existingField = await _formFieldService.GetFieldByIdAsync(fieldId);
        if (existingField == null || existingField.FormVersionId != version.Id)
        {
            return NotFound();
        }

        var result = await _formFieldService.DeleteFieldAsync(fieldId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("reorder")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> ReorderFormFields(Guid formId, int versionNumber, List<Guid> fieldIds)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        if (version == null)
        {
            return NotFound();
        }

        var result = await _formFieldService.ReorderFieldsAsync(version.Id, fieldIds);
        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

}

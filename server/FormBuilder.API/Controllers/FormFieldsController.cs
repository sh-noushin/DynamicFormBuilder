using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/forms/{formId}/versions/{versionNumber}/fields")]
[Authorize]
public class FormFieldsController : ControllerBase
{
    private readonly IFormVersionService _formVersionService;
    private readonly IFormFieldService _formFieldService;

    public FormFieldsController(IFormVersionService formVersionService, IFormFieldService formFieldService)
    {
        _formVersionService = formVersionService;
        _formFieldService = formFieldService;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<FormFieldDto>), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<IEnumerable<FormFieldDto>>> GetFormFields(Guid formId, int versionNumber)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        var fields = await _formFieldService.GetFieldsByVersionIdAsync(version.Id);
        return Ok(fields);
    }

    [HttpGet("{fieldId}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [ProducesResponseType(typeof(FormFieldDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormFieldDto>> GetFormField(Guid formId, int versionNumber, Guid fieldId)
    {
        var field = await ResolveFieldForVersionAsync(formId, versionNumber, fieldId);
        return Ok(field);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormFieldDto), 201)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormFieldDto>> CreateFormField(Guid formId, int versionNumber, CreateFormFieldDto createFieldDto)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        createFieldDto.FormVersionId = version.Id;
        var createdField = await _formFieldService.CreateFieldAsync(createFieldDto);
        return CreatedAtAction(nameof(GetFormField), new { formId, versionNumber, fieldId = createdField.Id }, createdField);
    }

    [HttpPut("{fieldId}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormFieldDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormFieldDto>> UpdateFormField(Guid formId, int versionNumber, Guid fieldId, UpdateFormFieldDto updateFieldDto)
    {
        var existingField = await ResolveFieldForVersionAsync(formId, versionNumber, fieldId);
        updateFieldDto.FormVersionId = existingField.FormVersionId;
        var updatedField = await _formFieldService.UpdateFieldAsync(fieldId, updateFieldDto);
        return Ok(updatedField);
    }

    [HttpDelete("{fieldId}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> DeleteFormField(Guid formId, int versionNumber, Guid fieldId)
    {
        await ResolveFieldForVersionAsync(formId, versionNumber, fieldId);
        await _formFieldService.DeleteFieldAsync(fieldId);
        return NoContent();
    }

    [HttpPost("reorder")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> ReorderFormFields(Guid formId, int versionNumber, List<Guid> fieldIds)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        var result = await _formFieldService.ReorderFieldsAsync(version.Id, fieldIds);
        if (!result)
            return BadRequest();
        return NoContent();
    }

    // Loads the field and verifies it belongs to the version identified by (formId, versionNumber).
    // Throws FormVersionNotFoundException / FormVersionFieldNotFoundException on any miss - the
    // middleware turns those into 404 responses.
    private async Task<FormFieldDto> ResolveFieldForVersionAsync(Guid formId, int versionNumber, Guid fieldId)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        var field = await _formFieldService.GetFieldByIdAsync(fieldId);
        if (field.FormVersionId != version.Id)
            throw new FormVersionFieldNotFoundException(fieldId);
        return field;
    }
}

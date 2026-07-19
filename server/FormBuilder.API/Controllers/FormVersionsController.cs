using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/forms/{formId}/versions")]
[Authorize]
public class FormVersionsController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly IFormVersionService _formVersionService;

    public FormVersionsController(IFormService formService, IFormVersionService formVersionService)
    {
        _formService = formService;
        _formVersionService = formVersionService;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<FormVersionDto>), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<IEnumerable<FormVersionDto>>> GetFormVersions(Guid formId)
    {
        // Throws FormNotFoundException if the form does not exist; handled by middleware.
        await _formService.GetFormByIdAsync(formId);
        var versions = await _formVersionService.GetVersionsByFormIdAsync(formId);
        return Ok(versions);
    }

    [HttpGet("{versionNumber}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [ProducesResponseType(typeof(FormVersionDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormVersionDto>> GetFormVersion(Guid formId, int versionNumber)
    {
        var version = await _formVersionService.GetVersionAsync(formId, versionNumber);
        return Ok(version);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormVersionDto), 201)]
    public async Task<ActionResult<FormVersionDto>> CreateFormVersion(Guid formId, CreateFormVersionDto createVersionDto)
    {
        var createdVersion = await _formVersionService.CreateVersionAsync(formId, createVersionDto);
        return CreatedAtAction(
            nameof(GetFormVersion),
            new { formId, versionNumber = createdVersion.VersionNumber },
            createdVersion);
    }

    [HttpPut("{versionNumber}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormVersionDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<FormVersionDto>> UpdateFormVersion(Guid formId, int versionNumber, UpdateFormVersionDto updateVersionDto)
    {
        var updatedVersion = await _formVersionService.UpdateVersionAsync(formId, versionNumber, updateVersionDto);
        return Ok(updatedVersion);
    }

    [HttpDelete("{versionNumber}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> DeleteFormVersion(Guid formId, int versionNumber)
    {
        await _formVersionService.DeleteVersionAsync(formId, versionNumber);
        return NoContent();
    }

    [HttpPost("{versionNumber}/publish")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> PublishFormVersion(Guid formId, int versionNumber)
    {
        await _formVersionService.PublishVersionAsync(formId, versionNumber);
        return NoContent();
    }

    [HttpPost("{versionNumber}/set-current")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> SetCurrentVersion(Guid formId, int versionNumber)
    {
        await _formVersionService.SetCurrentVersionAsync(formId, versionNumber);
        return NoContent();
    }
}

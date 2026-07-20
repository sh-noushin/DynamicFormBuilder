using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
// Class-level Bearer default (via [Authorize]). Individual GET endpoints
// opt in to the ApiKey scheme by overriding AuthenticationSchemes; write
// endpoints stay Bearer-only by construction.
[Authorize]
public class FormsController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly IFormAnalyticsService _analyticsService;

    public FormsController(IFormService formService, IFormAnalyticsService analyticsService)
    {
        _formService = formService;
        _analyticsService = analyticsService;
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer,ApiKey", Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<FormDto>), 200)]
    public async Task<IActionResult> GetAllForms()
    {
        var forms = await _formService.GetAllFormsAsync();
        return Ok(forms);
    }

    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer,ApiKey", Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> GetForm(Guid id)
    {
        var form = await _formService.GetFormByIdAsync(id);
        return Ok(form);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 201)]
    public async Task<IActionResult> CreateForm(CreateFormDto createFormDto)
    {
        var createdForm = await _formService.CreateFormAsync(createFormDto);
        return CreatedAtAction(nameof(GetForm), new { id = createdForm.Id }, createdForm);
    }

    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> UpdateForm(Guid id, UpdateFormDto updateFormDto)
    {
        var updatedForm = await _formService.UpdateFormAsync(id, updateFormDto);
        return Ok(updatedForm);
    }

    [HttpGet("{id}/analytics")]
    [Authorize(AuthenticationSchemes = "Bearer,ApiKey", Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormAnalyticsDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> GetFormAnalytics(Guid id, [FromQuery] int days = 30)
    {
        var analytics = await _analyticsService.GetFormAnalyticsAsync(id, days);
        return Ok(analytics);
    }

    [HttpPost("{id}/duplicate")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 201)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> DuplicateForm(Guid id)
    {
        var duplicated = await _formService.DuplicateFormAsync(id);
        return CreatedAtAction(nameof(GetForm), new { id = duplicated.Id }, duplicated);
    }

    [HttpGet("{id}/export")]
    [Authorize(AuthenticationSchemes = "Bearer,ApiKey", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormExportDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> ExportForm(Guid id)
    {
        var export = await _formService.ExportFormAsync(id);
        return Ok(export);
    }

    [HttpPost("import")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<IActionResult> ImportForm([FromBody] FormExportDto payload)
    {
        var created = await _formService.ImportFormAsync(payload);
        return CreatedAtAction(nameof(GetForm), new { id = created.Id }, created);
    }
}

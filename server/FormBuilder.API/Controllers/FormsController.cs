using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FormsController : ControllerBase
{
    private readonly IFormService _formService;

    public FormsController(IFormService formService)
    {
        _formService = formService;
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<FormDto>), 200)]
    public async Task<IActionResult> GetAllForms()
    {
        var forms = await _formService.GetAllFormsAsync();
        return Ok(forms);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AdminOrUser)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> GetForm(Guid id)
    {
        var form = await _formService.GetFormByIdAsync(id);
        return Ok(form);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 201)]
    public async Task<IActionResult> CreateForm(CreateFormDto createFormDto)
    {
        var createdForm = await _formService.CreateFormAsync(createFormDto);
        return CreatedAtAction(nameof(GetForm), new { id = createdForm.Id }, createdForm);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FormDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> UpdateForm(Guid id, UpdateFormDto updateFormDto)
    {
        var updatedForm = await _formService.UpdateFormAsync(id, updateFormDto);
        return Ok(updatedForm);
    }
}

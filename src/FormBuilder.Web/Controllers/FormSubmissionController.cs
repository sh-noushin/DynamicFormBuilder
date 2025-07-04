using FormBuilder.Application.Contract.FormSubmissions;
using FormBuilder.Application.Contract.FormSubmissions.Dtos.Request;
using FormBuilder.Application.Contract.FormSubmissions.Dtos.Response;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormSubmissionController : ControllerBase
{
    private readonly IFormSubmissionService _service;

    public FormSubmissionController(IFormSubmissionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFormSubmissionRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPost("{submissionId}/values")]
    public async Task<IActionResult> AddValue(Guid submissionId, CreateFormSubmissionValueRequest request)
    {
        await _service.AddValueAsync(submissionId, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FormSubmissionResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormSubmissionResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}
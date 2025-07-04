using FormBuilder.Application.Contract.FormSubmissionValues;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Request;
using FormBuilder.Application.Contract.FormSubmissionValues.Dtos.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormSubmissionValueController : ControllerBase
{
    private readonly IFormSubmissionValueService _service;

    public FormSubmissionValueController(IFormSubmissionValueService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFormSubmissionValueRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateFormSubmissionValueRequest request)
    {
        await _service.UpdateValueAsync(request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FormSubmissionValueResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormSubmissionValueResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}

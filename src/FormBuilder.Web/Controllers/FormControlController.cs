using FormBuilder.Application.Contract.FormControls;
using FormBuilder.Application.Contract.FormControls.Dtos.Request;
using FormBuilder.Application.Contract.FormControls.Dtos.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormControlController : ControllerBase
{
    private readonly IFormControlService _service;

    public FormControlController(IFormControlService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFormControlRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateFormControlRequest request)
    {
        await _service.UpdateAsync(request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FormControlResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-label/{label}")]
    public async Task<ActionResult<FormControlResponse>> GetByLabel(string label)
    {
        var result = await _service.GetByLabelAsync(label);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormControlResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}
using FormBuilder.Application.Contract.FormControlValues;
using FormBuilder.Application.Contract.FormControlValues.Dtos.Request;
using FormBuilder.Application.Contract.FormControlValues.Dtos.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormControlValueController : ControllerBase
{
    private readonly IFormControlValueService _service;

    public FormControlValueController(IFormControlValueService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFormControlValueRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateFormControlValueRequest request)
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
    public async Task<ActionResult<FormControlValueResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("GetByValue/{value}")]
    public async Task<ActionResult<FormControlValueResponse>> GetByValue(string value)
    {
        var result = await _service.GetByValueAsync(value);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormControlValueResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}

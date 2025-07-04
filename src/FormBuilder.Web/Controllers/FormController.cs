using FormBuilder.Application.Contract.Forms;
using FormBuilder.Application.Contract.Forms.Dtos.Request;
using FormBuilder.Application.Contract.Forms.Dtos.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormController : ControllerBase
{
    private readonly IFormService _service;

    public FormController(IFormService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFormRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateFormRequest request)
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

    [HttpPost("{formId}/versions")]
    public async Task<ActionResult<Guid>> AddVersion(Guid formId, [FromBody] string version)
    {
        var versionId = await _service.AddVersionAsync(formId, version);
        return Ok(versionId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FormResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<FormResponse>> GetByName(string name)
    {
        var result = await _service.GetByNameAsync(name);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}

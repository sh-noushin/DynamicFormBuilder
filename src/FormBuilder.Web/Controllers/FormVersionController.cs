using FormBuilder.Application.Contract.FormVersions;
using FormBuilder.Application.Contract.FormVersions.Dtos.Request;
using FormBuilder.Application.Contract.FormVersions.Dtos.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormVersionController : ControllerBase
{
    private readonly IFormVersionService _service;

    public FormVersionController(IFormVersionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFormVersionRequest request)
    {
        var id = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateFormVersionRequest request)
    {
        await _service.UpdateVersionAsync(request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FormVersionResponse>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-version/{version}")]
    public async Task<ActionResult<FormVersionResponse>> GetByVersion(string version)
    {
        var result = await _service.GetByVersionAsync(version);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FormVersionResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("getallwithcontrols")]
    public async Task<ActionResult<IEnumerable<FormVersionResponse>>> GetListWithControlsAsync()
    {
        var result = await _service.GetListWithControlsAsync();
        return Ok(result);
    }
}

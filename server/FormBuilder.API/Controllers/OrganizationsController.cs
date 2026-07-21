using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

// Super-admin console. Every endpoint requires the SuperAdmin role; a
// customer tenant admin can never reach these paths.
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.SuperAdmin)]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationsController(IOrganizationService service)
    {
        _service = service;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<OrganizationDto>), 200)]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> List()
    {
        var orgs = await _service.ListAsync();
        return Ok(orgs);
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OrganizationDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<ActionResult<OrganizationDto>> Create(CreateOrganizationDto payload)
    {
        var org = await _service.CreateAsync(payload);
        return CreatedAtAction(nameof(List), new { id = org.Id }, org);
    }

    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OrganizationDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<OrganizationDto>> Update(Guid id, UpdateOrganizationDto payload)
    {
        var org = await _service.UpdateAsync(id, payload);
        return Ok(org);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/impersonate")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ImpersonationResultDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<ImpersonationResultDto>> Impersonate(Guid id)
    {
        var result = await _service.ImpersonateAsync(id);
        return Ok(result);
    }
}

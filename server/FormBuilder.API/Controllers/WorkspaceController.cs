using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

// Tenant-admin surface for the caller's OWN workspace. Distinct from
// /api/organizations which is super-admin-only. A tenant admin cannot
// pass an org id — the service reads the current caller's org from the
// JWT claim, so cross-tenant access is impossible even by URL tampering.
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class WorkspaceController : ControllerBase
{
    private readonly IOrganizationService _service;

    public WorkspaceController(IOrganizationService service)
    {
        _service = service;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OrganizationDto), 200)]
    public async Task<ActionResult<OrganizationDto>> GetCurrent()
    {
        var dto = await _service.GetCurrentAsync();
        return Ok(dto);
    }

    [HttpPut]
    [Produces("application/json")]
    [ProducesResponseType(typeof(OrganizationDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<ActionResult<OrganizationDto>> Rename(UpdateOrganizationDto payload)
    {
        var dto = await _service.RenameCurrentAsync(payload);
        return Ok(dto);
    }
}

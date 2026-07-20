using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

// Admin-only CRUD for API keys. Deliberately locked to JWT (Bearer) so a
// leaked API key can never be used to mint or revoke keys - only real
// interactive admins do that.
[ApiController]
[Route("api/admin/api-keys")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<ApiKeyDto>), 200)]
    public async Task<ActionResult<IEnumerable<ApiKeyDto>>> List()
    {
        var keys = await _apiKeyService.ListAsync();
        return Ok(keys);
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiKeyCreatedDto), 201)]
    public async Task<ActionResult<ApiKeyCreatedDto>> Create([FromBody] CreateApiKeyDto payload)
    {
        var created = await _apiKeyService.CreateAsync(payload);
        return CreatedAtAction(nameof(List), created);
    }

    [HttpDelete("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiKeyDto), 200)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<ApiKeyDto>> Revoke(Guid id)
    {
        var revoked = await _apiKeyService.RevokeAsync(id);
        return Ok(revoked);
    }
}

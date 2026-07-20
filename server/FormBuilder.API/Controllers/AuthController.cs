using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResultDto), 200)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<ActionResult<LoginResultDto>> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { Message = "Logged out successfully" });
    }

    [HttpGet("user")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserInfoDto), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        var info = await _authService.GetCurrentUserAsync(User);
        return Ok(info);
    }
}

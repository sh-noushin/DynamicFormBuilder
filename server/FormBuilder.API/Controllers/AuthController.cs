using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordReset;

    public AuthController(IAuthService authService, IPasswordResetService passwordReset)
    {
        _authService = authService;
        _passwordReset = passwordReset;
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

    // Anonymous. Always returns 200 so an attacker can't tell whether
    // an email is registered.
    [HttpPost("request-reset")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetDto dto)
    {
        await _passwordReset.RequestResetAsync(dto.Email);
        return Ok(new { Message = "If that address is registered, a reset link is on the way." });
    }

    // Anonymous. Consumes the token from the emailed reset link and
    // sets the new password. Invalid or expired token → 401.
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        await _passwordReset.ResetAsync(dto.Email, dto.Token, dto.NewPassword);
        return Ok(new { Message = "Password reset. You can now log in with the new password." });
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

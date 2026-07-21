using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;

    public UserController(IUserService userService, IPasswordService passwordService)
    {
        _userService = userService;
        _passwordService = passwordService;
    }

    // Tenant admin creating a user inside their own workspace. The
    // request must be authenticated — the callers's org is read from
    // the JWT claim, so no orgId is accepted in the body.
    [HttpPost("register")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<ActionResult<UserDto>> RegisterUser(RegisterUserDto registerDto)
    {
        var userDto = await _userService.RegisterUserAsync(registerDto);
        return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
    }

    // Public tenant sign-up. Anonymous callers land as User role inside
    // the named tenant. Tenant admin shares this URL with their team.
    [HttpPost("register/{tenantSlug}")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<UserDto>> RegisterInTenant(string tenantSlug, RegisterUserDto registerDto)
    {
        var userDto = await _userService.RegisterInTenantAsync(tenantSlug, registerDto);
        return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var userDto = await _userService.GetUserByIdAsync(id);
        return Ok(userDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, UpdateUserDto updateDto)
    {
        var userDto = await _userService.UpdateUserAsync(id, updateDto);
        return Ok(userDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 401)]
    [ProducesResponseType(typeof(void), 404)]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "Unable to resolve the current user." });
        }

        await _passwordService.ChangePasswordAsync(userId, changePasswordDto);
        return Ok(new { Message = "Password changed successfully." });
    }
}

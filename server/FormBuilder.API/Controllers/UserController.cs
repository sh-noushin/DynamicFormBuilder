using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FormBuilder.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    
    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 500)]
    public async Task<ActionResult<UserDto>> RegisterUser(RegisterUserDto registerDto)
    {
        try
        {
            var userDto = await _userService.RegisterUserAsync(registerDto);
            return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
        }
        catch (DuplicateUsernameException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (DuplicateEmailException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (UserCreationFailedException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidUserRoleException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An internal error occurred while registering the user." });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
    [ProducesResponseType(typeof(void), 500)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An internal error occurred while retrieving users." });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    [ProducesResponseType(typeof(void), 500)]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        try
        {
            var userDto = await _userService.GetUserByIdAsync(id);
            return Ok(userDto);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An internal error occurred while retrieving the user." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    [ProducesResponseType(typeof(void), 500)]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, UpdateUserDto updateDto)
    {
        try
        {
            var userDto = await _userService.UpdateUserAsync(id, updateDto);
            return Ok(userDto);
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (DuplicateUsernameException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (DuplicateEmailException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (UserUpdateFailedException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidUserRoleException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An internal error occurred while updating the user." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), 204)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 404)]
    [ProducesResponseType(typeof(void), 500)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UserDeletionFailedException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An internal error occurred while deleting the user." });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(void), 400)]
    [ProducesResponseType(typeof(void), 401)]
    [ProducesResponseType(typeof(void), 404)]
    [ProducesResponseType(typeof(void), 500)]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "Unable to resolve the current user." });
        }

        try
        {
            await _userService.ChangePasswordAsync(userId, changePasswordDto);
            return Ok(new { Message = "Password changed successfully." });
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidCurrentPasswordException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (PasswordChangeFailedException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An internal error occurred while changing the password." });
        }
    }
}

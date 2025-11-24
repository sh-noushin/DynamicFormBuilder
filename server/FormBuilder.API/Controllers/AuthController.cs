using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly AutoMapper.IMapper _mapper;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IJwtService jwtService, AutoMapper.IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResultDto), 200)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<ActionResult<LoginResultDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username);
        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserDto>(user);
            var token = await _jwtService.GenerateTokenAsync(userDto, roles);

            return Ok(new LoginResultDto
            {
                Success = true,
                Username = userDto.Username,
                Email = userDto.Email,
                Roles = ConvertStringRolesToEnumRoles(roles),
                Token = token
            });
        }

        return Unauthorized(new { Message = "Invalid username or password" });
    }

    [HttpPost("logout")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "Logged out successfully" });
    }

    [HttpGet("user")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserInfoDto), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized();
        }

        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserInfoDto
        {
            Username = user.UserName!,
            Email = user.Email!,
            Roles = ConvertStringRolesToEnumRoles(roles)
        });
    }

    private static List<UserRole> ConvertStringRolesToEnumRoles(IList<string> stringRoles)
    {
        var enumRoles = new List<UserRole>();
        foreach (var role in stringRoles)
        {
            if (Enum.TryParse<UserRole>(role, out var enumRole))
            {
                enumRoles.Add(enumRole);
            }
        }
        return enumRoles;
    }
}

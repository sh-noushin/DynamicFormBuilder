using System.Security.Claims;
using AutoMapper;
using FormBuilder.Core.Common;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace FormBuilder.Core.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public AuthService(UserManager<User> userManager, IJwtService jwtService, IMapper mapper)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<LoginResultDto> LoginAsync(LoginDto loginDto)
    {
        if (loginDto == null)
            throw new ArgumentNullException(nameof(loginDto));

        var user = await _userManager.FindByNameAsync(loginDto.Username);
        if (user == null)
            throw new InvalidCredentialsException();

        var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
            throw new InvalidCredentialsException();

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserDto>(user);
        var token = await _jwtService.GenerateTokenAsync(userDto, roles);

        return new LoginResultDto
        {
            Success = true,
            Username = userDto.Username,
            Email = userDto.Email,
            Roles = RoleMapper.ToEnumRoles(roles),
            Token = token
        };
    }

    // With a JWT-based auth flow, logout is client-side (drop the token). This
    // exists so the controller can keep a consistent surface for cookie-based
    // sessions if they are ever re-enabled.
    public Task LogoutAsync() => Task.CompletedTask;

    public async Task<UserInfoDto> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userName = principal.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            throw new InvalidCredentialsException();

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            throw new UserNotFoundException(userName);

        var roles = await _userManager.GetRolesAsync(user);
        return new UserInfoDto
        {
            Username = user.UserName!,
            Email = user.Email!,
            Roles = RoleMapper.ToEnumRoles(roles)
        };
    }
}
